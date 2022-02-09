//using ReactiveUI;

//using System;

//using Windows.UI.Xaml;

//namespace ReactiveUIUnoSample.Converters
//{
//    // Note: Not needed; use ReactiveUI.Uno.BooleanToVisibilityTypeConverter together with ReactiveUI.Uno.BooleanToVisibilityHint if you want the inverse. Keeping in case it might be needed for Nullable<bool> because I haven't had a chance to see if the provided one handles that.
//    internal class BoolToVisibilityBindingTypeConverter : IBindingTypeConverter
//    {
//        public int GetAffinityForObjects(Type fromType, Type toType)
//        {
//            if ((fromType == typeof(bool) || fromType == typeof(bool?)) && toType == typeof(Visibility))
//            {
//                return 10;
//            }
//            return 0;
//        }

//        public bool TryConvert(object from, Type toType, object conversionHint, out object result)
//        {
//            if (toType == typeof(Visibility))
//            {
//                if (from is bool visible)
//                {
//                    result = visible ? Visibility.Visible : Visibility.Collapsed;
//                    return true;
//                }
//                else
//                {
//                    if (from is null)
//                    {
//                        result = Visibility.Collapsed;
//                        return true;
//                    }
//                }
//            }
//            result = Visibility.Collapsed;
//            return false;
//        }
//    }
//}
