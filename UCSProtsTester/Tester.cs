using NLog;
using OPCWrapper.DataAccess;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UCSProtsTester
{
    class Tester
    {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly OpcDaClient _opcDaClient;
        private readonly Settings _settings;
        private readonly DataRepository _repository;

        public Tester(OpcDaClient opcDaClient, Settings settings, DataRepository reportsRepository)
        {
            _opcDaClient = opcDaClient ?? throw new ArgumentNullException(nameof(opcDaClient));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _repository = reportsRepository ?? throw new ArgumentNullException(nameof(reportsRepository));
        }

        public async Task RunTest()
        {
            _logger.Info($"Start test (pairs count = {_settings.LogicalPressurePairs.Count})");
            var watcher = Stopwatch.StartNew();

            foreach (var logicalPressurePair in _settings.LogicalPressurePairs)
            {
                try
                {
                    var currentLogicalPressurePairReport = new LogicalPressurePairReport()
                    {
                        Name = _opcDaClient.ReadData(logicalPressurePair.ProtSourceInput + ".Description")?.Value?.ToString()
                    };

                    // Готовим логическую пару
                    if (!await PrepareLogicalPressurePair(logicalPressurePair))
                    {
                        currentLogicalPressurePairReport.Result = false;
                        currentLogicalPressurePairReport.Message = "Не удалось подготовить датчики давления к проверке";
                        _repository.Add(currentLogicalPressurePairReport);
                        continue;
                    }


                    // Имитируем правый датчик
                    if (!await ImitatePressurePointState(logicalPressurePair.RightPoint, PressurePointState.HiHi))
                    {
                        currentLogicalPressurePairReport.AddPairsReport(logicalPressurePair.LeftPoint, logicalPressurePair.RightPoint,
                            PressurePointState.None, PressurePointState.HiHi, false, "Не удалось сымитировать состояние правой точки давления из пары");
                        _repository.Add(currentLogicalPressurePairReport);
                        continue;
                    }

                    foreach (var prsState in Enum.GetValues(typeof(PressurePointState)).Cast<PressurePointState>())
                    {
                        try
                        {
                            if (prsState == PressurePointState.None)
                                continue;

                            // Имитируем левый датчик
                            if (!await ImitatePressurePointState(logicalPressurePair.LeftPoint, prsState))
                            {
                                currentLogicalPressurePairReport.AddPairsReport(logicalPressurePair.LeftPoint, logicalPressurePair.RightPoint,
                                    prsState, PressurePointState.HiHi, false, "Не удалось сымитировать состояние левой точки давления из пары");
                                _repository.Add(currentLogicalPressurePairReport);
                                continue;
                            }

                            // ждем срабатывание защиты в течение времени
                            await Task.Delay(_settings.ProtTriggeringWaitingDelay);

                            // Проверяем сработку защиты
                            var readResults = _opcDaClient.ReadData(new List<string> { logicalPressurePair.ProtSourceInput, logicalPressurePair.ProtSourceOutput }).ToList();
                            bool inputState = (readResults[0].IsSuccess && readResults[0].Value != null && readResults[0].Quality >= 192) && Convert.ToBoolean(readResults[0].Value);
                            bool outputState = (readResults[1].IsSuccess && readResults[1].Value != null && readResults[1].Quality >= 192) && Convert.ToBoolean(readResults[1].Value);

                            currentLogicalPressurePairReport.AddPairsReport(logicalPressurePair.LeftPoint, logicalPressurePair.RightPoint,
                                prsState, PressurePointState.HiHi, inputState && outputState, $"Проверка проведена успешно (inputState = {inputState}, outputState = {outputState})");

                            // восстанавливаем состояние левого датчика
                            if (!await RestorePressurePointState(logicalPressurePair.LeftPoint))
                            {
                                _logger.Error($"Cannot restore left point state. Break logical pressure pair test");
                                break;
                            }
                            await Task.Delay(_settings.ProtTriggeringWaitingDelay);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error($"Error checking prsState: {ex}");
                        }
                    }

                    // Восстанавливаем логическую пару
                    var restoreResult = await RestoreLogicalPressurePair(logicalPressurePair);
                    if (!restoreResult)
                    {
                        currentLogicalPressurePairReport.Result = false;
                        currentLogicalPressurePairReport.Message = "Не удалось восстановить состояние логической пары";
                    }

                    _repository.Add(currentLogicalPressurePairReport);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error checking logical pressure pair : {ex}");
                }
            }

            watcher.Stop();
            _logger.Info($"Test finished. Elapsed time = {watcher.Elapsed}");
        }

        private Task<bool> FillPoint(PressurePoint point)
        {
            var tcs = new TaskCompletionSource<bool>();
            // Считываем для точки давления нижний и верхний инж пределы, пред и авар давления (текущие), текущее значение
            var tagsList = new List<string>()
            {
                point.TagName + _settings.PostfixSettings.LowEng,
                point.TagName + _settings.PostfixSettings.HighEng,
                point.TagName + _settings.PostfixSettings.HiLimit,
                point.TagName + _settings.PostfixSettings.HiHiLimit,
                point.TagName,
                point.TagName + ".Description",
            };

            var values = _opcDaClient.ReadData(tagsList).ToList();
            var failed = values.Where(p => !p.IsSuccess || p.Quality < 192 || p.Value == null);
            if (failed.Count() > 0)
            {
                _logger.Error($"Some pressure point params read failed:");
                foreach (var fail in failed)
                {
                    _logger.Error($"Tag = {fail.ItemName}, Value = {fail.Value}, Quality = {fail.Quality}, Result = {fail.OperationResult}");
                }
                tcs.SetResult(false);
                return tcs.Task;
            }

            point.LowEng = Convert.ToDouble(values[0].Value);
            point.HighEng = Convert.ToDouble(values[1].Value);
            point.HiLimit = Convert.ToDouble(values[2].Value);
            point.HiHiLimit = Convert.ToDouble(values[3].Value);
            point.CurrentValue = Convert.ToDouble(values[4].Value); // текущее значение давления 
            point.Name = values[5].Value.ToString();

            tcs.SetResult(true);
            return tcs.Task;
        }

        /// <summary>
        /// Имтиация состояния точки давления через изменение нижнего предела
        /// </summary>
        /// <param name="point"></param>
        /// <param name="pressurePointState"></param>
        private Task<bool> ImitatePressurePointState(PressurePoint point, PressurePointState pressurePointState)
        {
            var tcs = new TaskCompletionSource<bool>();
            var linearCalculator = new LinearScaleCalculator(point.LowEng, 4, point.HighEng, 20);

            double imitateLowEng;
            if (pressurePointState == PressurePointState.Hi)
                imitateLowEng = linearCalculator.GetLowEng(point.HiLimit + 0.05, point.CurrentValue, point.HighEng);
            else if (pressurePointState == PressurePointState.HiHi)
                imitateLowEng = linearCalculator.GetLowEng(point.HiHiLimit + 0.05, point.CurrentValue, point.HighEng);
            else if (pressurePointState == PressurePointState.Undef)
                imitateLowEng = point.HighEng + 1;
            else
            {
                tcs.SetResult(false); // найти не удалось
                return tcs.Task;
            }

            if (double.IsNaN(imitateLowEng))
            {
                _logger.Error("Incorrect new LowEng for pressure point. Imitate skipping...");
                tcs.SetResult(false);
                return tcs.Task;
            }

            _logger.Info($"(ImitatePressurePointState) Writing: TagName = {point.TagName + _settings.PostfixSettings.LowEngTR}, value = {Math.Round(imitateLowEng, 3)}");
            var writeResult = _opcDaClient.WriteData(point.TagName + _settings.PostfixSettings.LowEngTR, Math.Round(imitateLowEng, 3));
            if (writeResult.IsSuccess)
                tcs.SetResult(true);
            else
            {
                _logger.Error($"Write new LowEng FAILED. Tag = {writeResult.ItemName}, Result = {writeResult.OperationResult}");
                _repository.Add(new CriticalErrorWriteTag() { TagName = writeResult.ItemName, TagValue = point.LowEng });
                tcs.SetResult(false);
            }

            return tcs.Task;
        }


        private Task<bool> RestorePressurePointState(PressurePoint point)
        {
            var tcs = new TaskCompletionSource<bool>();
            // восстанавливаем исходный нижний предел для датчика
            _logger.Info($"(RestorePressurePointState) Writing: TagName = {point.TagName + _settings.PostfixSettings.LowEngTR}, value = {point.LowEng}");
            var writeResult = _opcDaClient.WriteData(point.TagName + _settings.PostfixSettings.LowEngTR, point.LowEng);
            if (writeResult.IsSuccess)
            {
                tcs.SetResult(true);
            }
            else
            {
                _logger.Error($"Restore LowEng FAILED. Tag = {writeResult.ItemName}, Result = {writeResult.OperationResult}");
                _repository.Add(new CriticalErrorWriteTag() { TagName = writeResult.ItemName, TagValue = point.LowEng });
                tcs.SetResult(false);
            }
            return tcs.Task;
        }

        private Task<bool> SetPressurePointMeasurementChannel(PressurePoint point, MeasurementChannelType channelType)
        {
            var tcs = new TaskCompletionSource<bool>();

            string tagName = point.TagName;
            if (channelType == MeasurementChannelType.HART)
                tagName += _settings.PostfixSettings.SetHART;
            else if (channelType == MeasurementChannelType.TMA)
                tagName += _settings.PostfixSettings.SetTMA;
            else
            {
                tcs.SetResult(false);
                return tcs.Task;
            }

            _logger.Info($"(SetPressurePointMeasurementChannel) Writing: TagName = {point.TagName + _settings.PostfixSettings.LowEngTR}, value = {1}");
            var writeResult = _opcDaClient.WriteData(tagName, 1);
            if (writeResult.IsSuccess)
            {
                tcs.SetResult(true);
            }
            else
            {
                _logger.Error($"Change channel type FAILED. Tag = {writeResult.ItemName}, Result = {writeResult.OperationResult}");
                _repository.Add(new CriticalErrorWriteTag() { TagName = writeResult.ItemName, TagValue = point.LowEng });
                tcs.SetResult(false);
            }
            return tcs.Task;
        }

        private async Task<bool> PrepareLogicalPressurePair(LogicalPressurePair pressurePair)
        {
            // Устаналвиваем канал ТМА для обоих точек
            var taskList = new List<Task<bool>>()
            {
                // заполняем обе точки
                FillPoint(pressurePair.LeftPoint),
                FillPoint(pressurePair.RightPoint),

                SetPressurePointMeasurementChannel(pressurePair.LeftPoint, MeasurementChannelType.TMA),
                SetPressurePointMeasurementChannel(pressurePair.RightPoint, MeasurementChannelType.TMA)
            };
            await Task.WhenAll(taskList);

            return CheckBoolTaskList(taskList);
        }

        private async Task<bool> RestoreLogicalPressurePair(LogicalPressurePair pressurePair)
        {
            // Восстанавливаем канал HART для обоих точек
            var taskList = new List<Task<bool>>()
            {
                SetPressurePointMeasurementChannel(pressurePair.LeftPoint, MeasurementChannelType.HART),
                SetPressurePointMeasurementChannel(pressurePair.RightPoint, MeasurementChannelType.HART),
                RestorePressurePointState(pressurePair.LeftPoint),
                RestorePressurePointState(pressurePair.RightPoint)
            };
            await Task.WhenAll(taskList);

            return CheckBoolTaskList(taskList);
        }

        private bool CheckBoolTaskList(List<Task<bool>> taskList)
        {
            if (taskList.Any(p => !p.Result))
                return false;
            else
                return true;
        }
    }
}
