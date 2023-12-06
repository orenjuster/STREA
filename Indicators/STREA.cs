#region Using declarations
using System.Windows;
using System.Windows.Media;
using NinjaTrader.Cbi;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Data;
using Strea.Core;
using System.Windows.Controls;
using System.Text;
using NinjaTrader.Gui;
using NinjaTrader.Core.FloatingPoint;
using System;
using NinjaTrader.NinjaScript.MarketAnalyzerColumns;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
    public class STREA : Indicator
    {
        private static readonly object _streaDisplayLock = new object();
        private IStreaCalculator _streaCalculator = null;
        private StreaModel _lastSnapshot = new StreaModel();
        private StreaLevelOneModel _lastLevelOneSnapshot = new StreaLevelOneModel();
        private Grid _chartGrid;
        private ChartTab _chartTab;
        private Chart _chartWindow;
        private int _tabControlStartColumn;
        private TabItem _tabItem;
        private Button _resetButton;
        private Grid _rightSidePanelGrid;
        private GridSplitter _rightSidePanelGridSplitter;
        private bool _rightSidePanelActive;
        private TextBlock _streaDataLabel;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Display STREA data as specified by EYAL";
                Name = "STREA";
                DrawOnPricePanel = false;
                IsOverlay = false;
                IsSuspendedWhileInactive = true;
                Calculate = Calculate.OnEachTick;
            }
            else if (State == State.Historical)
            {
                _rightSidePanelActive = false;

                if (ChartControl != null)
                {
                    ChartControl.Dispatcher.InvokeAsync(() =>
                    {
                        // WPF modifications wait until State.Historical to play nice with duplicating tabs
                        CreateWPFControls();
                    });
                }
            }
            else if (State == State.Terminated)
            {
                if (ChartControl != null)
                {
                    ChartControl.Dispatcher.InvokeAsync((() =>
                    {
                        DisposeWPFControls();
                    }));
                }
            }
        }

        protected override void OnBarUpdate()
        {
            Value[0] = _lastSnapshot.Delta;
        }

        protected override void OnMarketData(MarketDataEventArgs marketDataUpdate)
        {
            var instrument = marketDataUpdate.Instrument;

            lock (_streaDisplayLock)
            {
                InitializeStreaCalculator(instrument);

                lock (instrument.SyncMarketData)
                {
                    _lastLevelOneSnapshot = _streaCalculator.CreateSnapshot(marketDataUpdate);
                }

                RefreshStreaDisplayData(); 
            }
        }

        protected override void OnMarketDepth(MarketDepthEventArgs marketDepthUpdate)
        {
            var instrument = marketDepthUpdate.Instrument;

            lock (_streaDisplayLock)
            {
                InitializeStreaCalculator(instrument);

                lock (Instrument.SyncMarketDepth)
                {
                    _lastSnapshot = _streaCalculator.CreateSnapshot(instrument.MarketDepth);
                }

                RefreshStreaDisplayData();
                
            }

            
        }

        private void InitializeStreaCalculator(Cbi.Instrument instrument)
        {
            if (_streaCalculator == null)
            {
                _streaCalculator = StreaCalculatorFactory.Create(instrument.MarketDepth);
            }
        }

        private void RefreshStreaDisplayData()
        {
            var streaText = new StringBuilder();

            streaText.AppendLine("STREA Data: ");
            streaText.AppendLine("Sum ASK: " + _lastSnapshot.SumAsk);
            streaText.AppendLine("Sum BID: " + _lastSnapshot.SumBid);
            streaText.AppendLine("Delta: " + _lastSnapshot.Delta);
            streaText.AppendLine("Min Delta: " + _lastSnapshot.MinimumDelta);
            streaText.AppendLine("Max Delta: " + _lastSnapshot.MaximumDelta);
            streaText.AppendLine("--- MKT ---");
            streaText.AppendLine("Delta: " + _lastLevelOneSnapshot.Delta);
            streaText.AppendLine("Min Delta: " + _lastLevelOneSnapshot.MinimumDelta);
            streaText.AppendLine("Max Delta: " + _lastLevelOneSnapshot.MaximumDelta);
            streaText.AppendLine("Delta2: " + _lastLevelOneSnapshot.DeltaBidMinusAsk);

            ChartControl.Dispatcher.InvokeAsync(() =>
            {
                _streaDataLabel.Text = streaText.ToString();
            });
        }

        private bool TabSelected()
        {
            return ChartControl != null && _chartWindow != null && _chartWindow.MainTabControl != null
&& ChartControl.ChartTab == ((_chartWindow.MainTabControl.Items.GetItemAt(_chartWindow.MainTabControl.SelectedIndex) as TabItem).Content as ChartTab);
        }

        protected void CreateWPFControls()
        {
            // the main chart window
            _chartWindow = System.Windows.Window.GetWindow(ChartControl.Parent) as Chart;
            // if not added to a chart, do nothing
            if (_chartWindow == null)
                return;
            // this is the grid in the chart window
            _chartGrid = _chartWindow.MainTabControl.Parent as Grid;

            // the buttons in the right panel will be within this grid
            _rightSidePanelGrid = new Grid() { };
            // this gridsplitter will allow the column our grid is in to be resized
            _rightSidePanelGridSplitter = new GridSplitter
            {
                Background = Brushes.Transparent,
                HorizontalAlignment = HorizontalAlignment.Left,
                ResizeBehavior = GridResizeBehavior.BasedOnAlignment,
                ResizeDirection = GridResizeDirection.Columns,
                VerticalAlignment = VerticalAlignment.Stretch,
                Width = 6
            };

            // add 3 rows for a row of text, a row for first button, a row for second button
            _rightSidePanelGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
            _rightSidePanelGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(300) });
            _rightSidePanelGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });

            var label = new TextBlock
            {
                FontFamily = ChartControl.Properties.LabelFont.Family,
                FontSize = 13,
                Foreground = ChartControl.Properties.ChartText,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(5, 5, 5, 5),
                Text = string.Format("{0} {1} {2}", Instrument.FullName, BarsPeriod.Value, BarsPeriod.BarsPeriodType)
            };

            Grid.SetRow(label, 0);
            _rightSidePanelGrid.Children.Add(label);

            _streaDataLabel = new TextBlock
            {
                FontFamily = ChartControl.Properties.LabelFont.Family,
                FontSize = 18,
                Foreground = ChartControl.Properties.ChartText,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(1, 1, 1, 1),
            };

            Grid.SetRow(_streaDataLabel, 1);
            _rightSidePanelGrid.Children.Add(_streaDataLabel);

            _resetButton = new Button
            {
                Content = "Reset Data",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            _resetButton.Click += _resetButton_Click;

            Grid.SetRow(_resetButton, 2);
            _rightSidePanelGrid.Children.Add(_resetButton);

            if (TabSelected())
                ShowWPFControls();

            _chartWindow.MainTabControl.SelectionChanged += TabChangedHandler;
        }

        private void _resetButton_Click(object sender, RoutedEventArgs e)
        {
            _streaCalculator = null;
            _lastSnapshot = new StreaModel();
            _lastLevelOneSnapshot = new StreaLevelOneModel();
            RefreshStreaDisplayData();
        }

        private void TabChangedHandler(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count <= 0)
                return;

            _tabItem = e.AddedItems[0] as TabItem;
            if (_tabItem == null)
                return;

            _chartTab = _tabItem.Content as ChartTab;
            if (_chartTab == null)
                return;

            if (TabSelected())
                ShowWPFControls();
            else
                HideWPFControls();
        }

        private void HideWPFControls()
        {
            if (!_rightSidePanelActive)
            {
                return;
            }

            // remove the column of our added grid
            _chartGrid.ColumnDefinitions.RemoveAt(Grid.GetColumn(_rightSidePanelGrid));
            // then remove the grid and gridsplitter
            _chartGrid.Children.Remove(_rightSidePanelGrid);
            _chartGrid.Children.Remove(_rightSidePanelGridSplitter);

            // if the childs column is 1 (so we can move it to 0) and the column is to the right of the column we are removing, shift it left
            for (int i = 0; i < _chartGrid.Children.Count; i++)
                if (Grid.GetColumn(_chartGrid.Children[i]) > 0 && Grid.GetColumn(_chartGrid.Children[i]) > Grid.GetColumn(_rightSidePanelGrid))
                    Grid.SetColumn(_chartGrid.Children[i], Grid.GetColumn(_chartGrid.Children[i]) - 1);

            _rightSidePanelActive = false;
        }

        private void ShowWPFControls()
        {
            if (_rightSidePanelActive)
            {
                return;
            }

            _tabControlStartColumn = Grid.GetColumn(_chartWindow.MainTabControl);

            // a new column is added to the right of MainTabControl
            _chartGrid.ColumnDefinitions.Insert((_tabControlStartColumn + 1), new ColumnDefinition()
            {
                // The width will need to be GridUnitType.Star to work with the gridsplitter from chartTrader (as well as our own)
                // The width set here is a ratio to other star columns (such as when we make the chart column starred below)
                Width = new GridLength(1, GridUnitType.Star),
                // the minimum width should at least be big enough to grab our added gridspliiter with mouse
                MinWidth = 10
            });

            // all items to the right of the MainTabControl are shifted to the right
            for (int i = 0; i < _chartGrid.Children.Count; i++)
                if (Grid.GetColumn(_chartGrid.Children[i]) > _tabControlStartColumn)
                    Grid.SetColumn(_chartGrid.Children[i], Grid.GetColumn(_chartGrid.Children[i]) + 1);

            // and then we set our new grid to be within the new column of the chart grid (and on the same row as the MainTabControl)
            Grid.SetColumn(_rightSidePanelGrid, Grid.GetColumn(_chartWindow.MainTabControl) + 1);
            Grid.SetRow(_rightSidePanelGrid, Grid.GetRow(_chartWindow.MainTabControl));

            _chartGrid.Children.Add(_rightSidePanelGrid);

            // add a grid splitter to the same column as our side panel grid to allow us to resize the width of our panel
            Grid.SetColumn(_rightSidePanelGridSplitter, Grid.GetColumn(_rightSidePanelGrid));
            Grid.SetRow(_rightSidePanelGridSplitter, Grid.GetRow(_rightSidePanelGrid));

            _chartGrid.Children.Add(_rightSidePanelGridSplitter);

            // to work with the added gridsplitter, the chart column to the left needs to be width star and larger than our panel
            _chartGrid.ColumnDefinitions[Grid.GetColumn(_chartWindow.MainTabControl)].Width = new GridLength(5, GridUnitType.Star);
            _rightSidePanelActive = true;
        }

        private void DisposeWPFControls()
        {
            if (_chartWindow != null)
                _chartWindow.MainTabControl.SelectionChanged -= TabChangedHandler;

            HideWPFControls();

            if (_resetButton != null)
                _resetButton.Click -= _resetButton_Click;
        }
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private STREA[] cacheSTREA;
		public STREA STREA()
		{
			return STREA(Input);
		}

		public STREA STREA(ISeries<double> input)
		{
			if (cacheSTREA != null)
				for (int idx = 0; idx < cacheSTREA.Length; idx++)
					if (cacheSTREA[idx] != null &&  cacheSTREA[idx].EqualsInput(input))
						return cacheSTREA[idx];
			return CacheIndicator<STREA>(new STREA(), input, ref cacheSTREA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.STREA STREA()
		{
			return indicator.STREA(Input);
		}

		public Indicators.STREA STREA(ISeries<double> input )
		{
			return indicator.STREA(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.STREA STREA()
		{
			return indicator.STREA(Input);
		}

		public Indicators.STREA STREA(ISeries<double> input )
		{
			return indicator.STREA(input);
		}
	}
}

#endregion
