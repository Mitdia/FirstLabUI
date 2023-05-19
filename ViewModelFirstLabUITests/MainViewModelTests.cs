using Xunit;
using ViewModelFirstLabUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using FluentAssertions;


namespace ViewModelFirstLabUITests
{
    public class MainViewModelTests
    {

        [Fact]
        public void ErrorScenario()
        {
            var uiServices = new Mock<IUIServices>();
            FileStream? fileStream = null;
            try
            {
                fileStream = File.Create("emty_tmp.json");
            }
            finally
            {
                fileStream?.Close();
            }
            uiServices.Setup(w => w.ChooseFileToOpen()).Returns("emty_tmp.json");
            var viewModel = new MainViewModel(uiServices.Object);
            viewModel.NumberOfPoints = 15;
            viewModel.ExecuteFromFileCommand.Execute(null);
            uiServices.Verify(r => r.ReportError("Load failed because: The loaded raw data has no Force Values array"), Times.Once());
            uiServices.Verify(r => r.ReportError("Interpolation failed because: Raw data can't be interpolated, because it has no points or is corrupted"), Times.Once());
        }

        [Fact]
        public void BasicScenario()
        {
            var uiServices = new Mock<IUIServices>();
            uiServices.Setup(w => w.ChooseFileToSave()).Returns("tmp.json");
            uiServices.Setup(w => w.ChooseFileToOpen()).Returns("tmp.json");
            var viewModel =  new MainViewModel(uiServices.Object);
            viewModel.ExecuteFromDataCommand.CanExecute(null).Should().BeFalse();
            viewModel.ExecuteFromFileCommand.CanExecute(null).Should().BeFalse();
            viewModel.SaveCommand.CanExecute(null).Should().BeFalse();
            viewModel.ForceValues.Should().BeNull();
            viewModel.SplineValues.Should().BeNull();

            viewModel.NumberOfPoints = 15;
            viewModel.ExecuteFromDataCommand.CanExecute(null).Should().BeFalse();
            viewModel.ExecuteFromFileCommand.CanExecute(null).Should().BeTrue();
            viewModel.SaveCommand.CanExecute(null).Should().BeFalse();

            viewModel.NumberOfInitialPoints = 5;
            viewModel.ExecuteFromDataCommand.CanExecute(null).Should().BeTrue();
            viewModel.ExecuteFromFileCommand.CanExecute(null).Should().BeTrue();
            viewModel.SaveCommand.CanExecute(null).Should().BeFalse();

            viewModel.FirstDerivativeOnLeftSegmentEnd = 1;
            viewModel.FirstDerivativeOnRightSegmentEnd = 1;
            // Test pushing ExecuteFromData
            viewModel.ExecuteFromDataCommand.Execute(null);
            viewModel.ForceValues.Should().NotBeNull();
            viewModel.ForceValues.Should().HaveCount(5);
            viewModel.ForceValues.Should().OnlyContain(point => Math.Abs(point.Coordinate - point.Force) < 1e-5);

            viewModel.SplineValues.Should().NotBeNull();
            viewModel.SplineValues.Should().HaveCount(15);
            viewModel.SplineValues.Should().OnlyContain(point => Math.Abs(point.PointCoordinate - point.SplineValue) < 1e-5);
            viewModel.SplineValues.Should().OnlyContain(point => Math.Abs(point.FirstDerivativeValue - 1.0) < 1e-5);
            viewModel.SplineValues.Should().OnlyContain(point => Math.Abs(point.SecondDerivativeValue) < 1e-5);

            viewModel.IntegralValue.Should().Be(1.5);


            // Test pushing ExecuteFromFile
            viewModel.SaveCommand.Execute(null);
            viewModel.ExecuteFromFileCommand.Execute(null);
            uiServices.Verify(r => r.ReportError(It.IsAny<string>()), Times.Never);
            uiServices.Verify(p => p.Plot(It.IsAny<double[]>(), It.IsAny<double[]>(), "raw"), Times.Once());
            uiServices.Verify(f => f.ChooseFileToOpen(), Times.Once());
            uiServices.Verify(f => f.ChooseFileToSave(), Times.Once());
            uiServices.Verify(p => p.Plot(It.IsAny<double[]>(), It.IsAny<double[]>(), "spline"), Times.Exactly(2));
        }
    }
}