using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GraphicalMirai
{
    /// <summary>
    /// IntegerTextBox.xaml 的交互逻辑
    /// </summary>
    public partial class IntegerTextBox : UserControl
    {
        private static DependencyProperty RegisterProperty<Value>(string name, Value? defaultValue, Action<IntegerTextBox, DependencyPropertyChangedEventArgs> onValueChanged) =>
            DependencyProperty.Register(name, typeof(Value), typeof(IntegerTextBox),
                new(defaultValue, new((sender, e) => onValueChanged((IntegerTextBox)sender, e))));

        public event EventHandler<DependencyPropertyChangedEventArgs> ValueChanged;
        public IntegerTextBox()
        {
            ValueChanged += delegate { };
            InitializeComponent();
        }
        public static readonly DependencyProperty ValueProperty = RegisterProperty(name: "Value", defaultValue: 0,
            onValueChanged: (sender, e) =>
            {
                sender.InputBox.Text = e.NewValue.ToString();
                sender.ValueChanged(sender, e);
            }
        );
        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { if (value >= Minimum && value <= Maximum) SetValue(ValueProperty, value); }
        }
        public static readonly DependencyProperty ValueStepProperty = RegisterProperty(name: "ValueStep", defaultValue: 1,
            (_, _) => { }
        );
        public int ValueStep
        {
            get { return (int)GetValue(ValueStepProperty); }
            set { SetValue(ValueStepProperty, value); }
        }

        public static readonly DependencyProperty MaximumProperty = RegisterProperty(name: "Maximum", defaultValue: int.MaxValue,
            (_, _) => { }
        );
        public int Maximum
        {
            get { return (int)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public static readonly DependencyProperty MinimumProperty = RegisterProperty(name: "Minimum", defaultValue: int.MinValue,
            (_, _) => { }
        );
        public int Minimum
        {
            get { return (int)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        private void ButtonUp_Click(object sender, RoutedEventArgs e)
        {
            int oldValue = Value;
            int newValue = Math.Min(Maximum, oldValue + ValueStep);
            if (oldValue != newValue) Value = newValue;
        }
        private void ButtonDown_Click(object sender, RoutedEventArgs e)
        {
            int oldValue = Value;
            int newValue = Math.Max(Minimum, oldValue - ValueStep);
            if (oldValue != newValue) Value = newValue;
        }

        private void InputBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(InputBox.Text, out int newValue))
            {
                if (newValue >= Minimum && newValue <= Maximum) Value = newValue;
                else InputBox.Text = Value.ToString();
            }
            else InputBox.Text = Value.ToString();
        }

        private void InputBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out int _);
        }
        string lastText = "";
        private void InputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (InputBox.Text == lastText) return;
            if (lastText.Length > 0 && !int.TryParse(InputBox.Text, out int _))
            {
                InputBox.Text = lastText;
            }
            else lastText = InputBox.Text;
        }
    }
}
