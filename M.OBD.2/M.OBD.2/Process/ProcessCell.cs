
using Xamarin.Forms;

namespace M.OBD2
{
    public class ProcessCell : ViewCell
    {
        public ProcessCell()
        {
            // Create grid with columns for process display
            Grid grid = new Grid
            {
                Padding = new Thickness(5, 10, 0, 0),
                ColumnDefinitions =
                  {
                    new ColumnDefinition { Width = new GridLength(10, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(10, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(10, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(10, GridUnitType.Star)  }
                  },
            };

            // Create column objects and bindings
            Image imgStatus = new Image()
            {
                HorizontalOptions = LayoutOptions.Start,
                HeightRequest = 48
            };
            imgStatus.SetBinding(Image.SourceProperty, new Binding("ImageSource"));

            Label lblName = new Label()
            {
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalTextAlignment = TextAlignment.End,
            };
            lblName.SetBinding(Label.TextProperty, new Binding("Name"));
            lblName.SetBinding(Label.TextColorProperty, new Binding("NameColor"));

            Label lblValue = new Label()
            {
                VerticalTextAlignment = TextAlignment.Center
            };
            lblValue.SetBinding(Label.TextProperty, new Binding("Value"));
            lblValue.SetBinding(Label.TextColorProperty, new Binding("ValueColor"));

            Label lblUnits = new Label()
            {
                VerticalTextAlignment = TextAlignment.Center
            };
            lblUnits.SetBinding(Label.TextProperty, new Binding("Units"));
            lblUnits.SetBinding(Label.TextColorProperty, new Binding("UnitsColor"));

            // Create view with child objects
            View = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.StartAndExpand,
                Padding = new Thickness(15, 5, 5, 15),
                Children =
                {
                      new StackLayout
                      {
                        Orientation = StackOrientation.Horizontal,
                        Children = { imgStatus,lblName, lblValue, lblUnits }
                      },
                }
            };
        }
    }
}