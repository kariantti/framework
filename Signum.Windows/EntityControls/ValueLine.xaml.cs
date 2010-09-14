﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Signum.Entities.Basics;
using Signum.Utilities;

namespace Signum.Windows
{
    

    /// <summary>
    /// Utiliza una deduccion de propiedades muy agresiva:
    /// Value (binding) -> ValueType -> ValueLineType -> ValueControl
    /// </summary>
    public partial class ValueLine : LineBase
    {
        public static readonly DependencyProperty UnitTextProperty =
            DependencyProperty.Register("UnitText", typeof(string), typeof(ValueLine), new UIPropertyMetadata(null));
        public string UnitText
        {
            get { return (string)GetValue(UnitTextProperty); }
            set { SetValue(UnitTextProperty, value); }
        }

        public static readonly DependencyProperty FormatProperty =
         DependencyProperty.Register("Format", typeof(string), typeof(ValueLine), new UIPropertyMetadata(null));
        public string Format
        {
            get { return (string)GetValue(FormatProperty); }
            set { SetValue(FormatProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(ValueLine), new UIPropertyMetadata(null));
        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueLineTypeProperty =
            DependencyProperty.Register("ValueLineType", typeof(ValueLineType), typeof(ValueLine), new UIPropertyMetadata(ValueLineType.String));
        public ValueLineType ValueLineType
        {
            get { return (ValueLineType)GetValue(ValueLineTypeProperty); }
            set { SetValue(ValueLineTypeProperty, value); }
        }

        public static readonly DependencyProperty ValueControlProperty =
            DependencyProperty.Register("ValueControl", typeof(Control), typeof(ValueLine), new UIPropertyMetadata(null));
        public Control ValueControl
        {
            get { return (Control)GetValue(ValueControlProperty); }
            set { SetValue(ValueControlProperty, value); }
        }

        public static readonly DependencyProperty ItemSourceProperty =
            DependencyProperty.Register("ItemSource", typeof(IEnumerable), typeof(ValueLine), new UIPropertyMetadata(null));
        public IEnumerable ItemSource
        {
            get { return (IEnumerable)GetValue(ItemSourceProperty); }
            set { SetValue(ItemSourceProperty, value); }
        }

        public ValueLine()
        {
            InitializeComponent();
        }

        public override void OnLoad(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            base.OnLoad(sender, e);

            if (this.NotSet(ValueLineTypeProperty))
                this.ValueLineType = Configurator.GetDefaultValueLineType(this.Type);

            this.ValueControl = this.CreateControl();

            this.label.Target = this.ValueControl;
        }

        protected internal override DependencyProperty CommonRouteValue()
        {
            return ValueProperty;
        }


        public static ValueLineConfigurator Configurator = new ValueLineConfigurator(); 

   
        private Control CreateControl()
        {
            Control control = Configurator.constructor[ValueLineType](this);
            if(Configurator.SetToolTipStyle(this))
              control.Style = (Style)FindResource("toolTip"); 
            
            Binding b; 
            BindingExpression bindingExpression = BindingOperations.GetBindingExpression(this, ValueProperty);
            if (bindingExpression != null) // is something is binded to ValueProperty, bind the new control to there
            {
                Binding binding = bindingExpression.ParentBinding;
                Validation.ClearInvalid(bindingExpression);
                BindingOperations.ClearBinding(this, ValueProperty);
                b = new Binding(binding.Path.Path)
                {
                    UpdateSourceTrigger =  Configurator.GetUpdateSourceTrigger(this),
                    Mode = binding.Mode,
                    ValidatesOnExceptions = true,
                    ValidatesOnDataErrors = true,
                    NotifyOnValidationError = true,
                    Converter = binding.Converter,
                };
            }
            else //otherwise bind to value property
            {
                b = new Binding()
                {
                    Path = new PropertyPath(ValueLine.ValueProperty),
                    Source = this,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    Mode = BindingMode.TwoWay,
                };
            }

            if (b.Converter == null)
                b.Converter = Configurator.GetConverter(this);

            ValidationRule validation = Configurator.GetValidation(this);
            if (validation != null)
                b.ValidationRules.Add(validation);

            DependencyProperty prop = Configurator.properties[this.ValueLineType];
       
            control.SetBinding(prop, b);

            Binding rb = new Binding
            {
                Source = this,
                Path = new PropertyPath(Common.IsReadOnlyProperty),
                Mode = BindingMode.OneWay,
                Converter = Configurator.GetReadOnlyConverter(this)
            };

            control.SetBinding(Configurator.readOnlyProperties[this.ValueLineType], rb);  
            // Binding b = new Binding(binding.Path.Path) { Mode = binding.Mode, UpdateSourceTrigger = binding.UpdateSourceTrigger };

            //System.Diagnostics.PresentationTraceSources.SetTraceLevel(b, PresentationTraceLevel.High);
   
            return control;
        }
    }

    public class ValueLineConfigurator
    {
        static DataTemplate comboDataTemplate;

        static ValueLineConfigurator()
        {
            Binding b = new Binding() { Mode = BindingMode.OneTime, Converter = Converters.EnumDescriptionConverter };
            System.Diagnostics.PresentationTraceSources.SetTraceLevel(b, PresentationTraceLevel.High);
            comboDataTemplate = new DataTemplate
            {
                VisualTree = new FrameworkElementFactory(typeof(TextBlock))
                        .Do(f => f.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Right))
                        .Do(f => f.SetBinding(TextBlock.TextProperty, b))
            };
        }


        public virtual ValueLineType GetDefaultValueLineType(Type type)
        {
            type = type.UnNullify();

            if (type.IsEnum)
                return ValueLineType.Enum;
            else if (type == typeof(ColorDN))
                return ValueLineType.Color;
            else
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Boolean:
                        return ValueLineType.Boolean;
                    case TypeCode.Double:
                    case TypeCode.Decimal:
                    case TypeCode.Single:
                    case TypeCode.Byte:
                    case TypeCode.SByte:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                        return ValueLineType.Number;
                    case TypeCode.DateTime:
                        return ValueLineType.DateTime;
                    case TypeCode.Empty:
                    case TypeCode.Object:
                    case TypeCode.Char:
                    case TypeCode.String:
                    default:
                        return ValueLineType.String;
                }        
            }
        }

        public Dictionary<ValueLineType, DependencyProperty> properties = new Dictionary<ValueLineType, DependencyProperty>()
        {
            {ValueLineType.Enum, ComboBox.SelectedItemProperty},
            {ValueLineType.Boolean,CheckBox.IsCheckedProperty},
            {ValueLineType.Number, NumericTextBox.ValueProperty},
            {ValueLineType.String, TextBox.TextProperty},
            {ValueLineType.DateTime, DateTimePicker.SelectedDateProperty},
            {ValueLineType.Color, ColorPicker.SelectedColorProperty},
        };

        public Dictionary<ValueLineType, DependencyProperty> readOnlyProperties = new Dictionary<ValueLineType, DependencyProperty>()
        {
            {ValueLineType.Enum, ComboBox.IsEnabledProperty},
            {ValueLineType.Boolean,CheckBox.IsEnabledProperty},
            {ValueLineType.Number, NumericTextBox.IsReadOnlyProperty},
            {ValueLineType.String, TextBox.IsReadOnlyProperty},
            {ValueLineType.DateTime, DateTimePicker.IsReadOnlyProperty},
            {ValueLineType.Color, ColorPicker.IsReadOnlyProperty}
        };

        public Dictionary<ValueLineType, Func<ValueLine, Control>> constructor = new Dictionary<ValueLineType, Func<ValueLine, Control>>()
        {
            {ValueLineType.Enum, vl =>new ComboBox()
            { 
                ItemsSource = vl.ItemSource ??  EnumExtensions.UntypedGetValues(vl.Type.UnNullify()).PreAndNull(vl.Type.IsNullable()),
                ItemTemplate = comboDataTemplate, 
                VerticalContentAlignment = VerticalAlignment.Center
            }},
            {ValueLineType.Boolean, vl =>new CheckBox(){ VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Left}},
            {ValueLineType.Number, vl =>
            {
                var nt = new NumericTextBox(){ XIncrement = 10, YIncrement = 1};
                if(vl.Format != null)
                {
                    var format = NullableDecimalConverter.NormalizeToDecimal(vl.Format);

                    nt.NullableDecimalConverter = 
                        format == NullableDecimalConverter.Integer.Format?  NullableDecimalConverter.Integer:
                        format == NullableDecimalConverter.Number.Format?  NullableDecimalConverter.Number:
                        new NullableDecimalConverter(format); 
                }
                return nt;
            }},
            {ValueLineType.String, vl => 
                {
                    var tb = new TextBox();
                    if(vl.Format == "U")
                        tb.CharacterCasing = CharacterCasing.Upper;
                    else if(vl.Format == "L")
                        tb.CharacterCasing = CharacterCasing.Lower;
                    return tb;
                }
            },
            {ValueLineType.DateTime, vl => 
            {
                var dt = new DateTimePicker(); 
                if(vl.Format != null) 
                {
                    dt.DateTimeConverter = 
                        vl.Format == DateTimeConverter.DateAndTime.Format?  DateTimeConverter.DateAndTime:
                        vl.Format == DateTimeConverter.Date.Format?  DateTimeConverter.Date:
                        new DateTimeConverter(vl.Format); 
                }
                return dt;
            }},
            {ValueLineType.Color, vl => new ColorPicker()}
        };


        public virtual IValueConverter GetConverter(ValueLine vl)
        {
            if (vl.ValueLineType == ValueLineType.Enum && vl.Type.IsNullable())
                return Converters.NullableEnumConverter;

            if (vl.ValueLineType == ValueLineType.Color)
                return Converters.ColorConverter;

            if (vl.Type.IsNullable())
                return Converters.Identity;

            return null;
        }


        public virtual ValidationRule GetValidation(ValueLine vl)
        {
            if (vl.Type.IsValueType && !vl.Type.IsNullable())
                return NotNullValidationRule.Instance;

            return null;
        }

        public virtual bool SetToolTipStyle(ValueLine vl)
        {
            if (vl.ValueLineType == ValueLineType.String)
                return false;

            return true; 
        }

        public virtual UpdateSourceTrigger GetUpdateSourceTrigger(ValueLine vl)
        {
            return UpdateSourceTrigger.PropertyChanged;
        }

        public virtual IValueConverter GetReadOnlyConverter(ValueLine vl)
        {
            if (vl.ValueLineType == ValueLineType.Boolean || vl.ValueLineType == ValueLineType.Enum)
                return Converters.Not;

            return null;
        }
    }

    public enum ValueLineType
    {
        Enum,
        Boolean,
        Number,
        String,
        DateTime,
        Color
    };
}
