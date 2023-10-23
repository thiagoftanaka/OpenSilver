
/*===================================================================================
* 
*   Copyright (c) Userware/OpenSilver.net
*      
*   This file is part of the OpenSilver Runtime (https://opensilver.net), which is
*   licensed under the MIT license: https://opensource.org/licenses/MIT
*   
*   As stated in the MIT license, "the above copyright notice and this permission
*   notice shall be included in all copies or substantial portions of the Software."
*  
\*====================================================================================*/

using System.Globalization;
using CSHTML5.Internal;

namespace System.Windows.Media
{
    /// <summary>
    /// Defines functionality that enables transformations in a two-dimensional plane.
    /// </summary>
    public abstract class Transform : GeneralTransform
    {
        internal Transform() { }

        ///<summary>
        /// Return the current transformation value.
        ///</summary>
        internal abstract Matrix ValueInternal { get; }

        ///<summary>
        /// Returns true if transformation if the transformation is definitely an identity.  There are cases where it will
        /// return false because of computational error or presence of animations (And we're interpolating through a
        /// transient identity) -- this is intentional.  This property is used internally only.  If you need to check the
        /// current matrix value for identity, use Transform.Value.Identity.
        ///</summary>
        internal abstract bool IsIdentity { get; }

        /// <summary>
        /// Attempts to transform the specified point and returns a value that indicates
        /// whether the transformation was successful.
        /// </summary>
        /// <param name="inPoint">
        /// The point to transform.
        /// </param>
        /// <param name="outPoint">
        /// The result of transforming inPoint.
        /// </param>
        /// <returns>
        /// true if inPoint was transformed; otherwise, false.
        /// </returns>
        public override bool TryTransform(Point inPoint, out Point outPoint)
        {
            Matrix m = ValueInternal;
            outPoint = m.Transform(inPoint);
            return true;
        }

        public override Rect TransformBounds(Rect rect)
        {
            Matrix matrix = ValueInternal;
            MatrixUtil.TransformRect(ref rect, ref matrix);
            return rect;
        }

        /// <summary>
        /// Gets the inverse of this transform, if it exists.
        /// </summary>
        /// <returns>
        /// The inverse of this transform, if it exists; otherwise, null.
        /// </returns>
        public override GeneralTransform Inverse
        {
            get
            {
                Matrix matrix = ValueInternal;
                if (!matrix.HasInverse)
                {
                    return null;
                }

                matrix.Invert();
                return new MatrixTransform(matrix);
            }
        }

        /// <summary>
        /// Parse - returns an instance converted from the provided string
        /// using the current culture
        /// <param name="source"> string with Transform data </param>
        /// </summary>
        internal static Transform Parse(string source)
        {
            Matrix matrix = Matrix.Parse(source);

            return new MatrixTransform(matrix);
        }

        // Must be implemented by the concrete class:
        internal abstract void INTERNAL_ApplyTransform();
        internal abstract void INTERNAL_UnapplyTransform();

        internal event EventHandler Changed;

        internal void RaiseTransformChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }

        internal static void ProcessChanged(UIElement uiElement, Transform oldValue, Transform newValue)
        {
            if (oldValue != null)
            {
                oldValue.INTERNAL_UnapplyTransform();
                oldValue.INTERNAL_parent = null;
            }
            if (uiElement != null && INTERNAL_VisualTreeManager.IsElementInVisualTree(uiElement))
            {
                if (newValue != null)
                {
                    newValue.INTERNAL_parent = uiElement;
                    if (newValue is TransformGroup transformGroup)
                    {
                        foreach (var child in transformGroup.Children)
                        {
                            child.INTERNAL_parent = uiElement;
                        }
                    }
                    newValue.INTERNAL_ApplyTransform();

                    // Ensure that the default RenderTransformOrigin is (0,0) like in normal XAML, instead of (0.5,0.5) like in CSS:
                    if (!uiElement.INTERNAL_RenderTransformOriginHasBeenApplied)
                        ApplyRenderTransformOrigin(uiElement, new Point(0d, 0d));
                }
                else
                {
                    var domStyle = INTERNAL_HtmlDomManager.GetFrameworkElementOuterStyleForModification(uiElement);

                    try
                    {
                        domStyle.transform = "";
                    }
                    catch
                    {
                    }
                }
            }
        }

        internal static void ApplyRenderTransformOrigin(UIElement uiElement, Point newValue)
        {
            var domStyle = INTERNAL_HtmlDomManager.GetFrameworkElementOuterStyleForModification(uiElement);
            string transformOriginValue = $"{(newValue.X * 100).ToString(CultureInfo.InvariantCulture)}% {(newValue.Y * 100).ToString(CultureInfo.InvariantCulture)}%";

            try
            {
                domStyle.transformOrigin = transformOriginValue;
            }
            catch
            {
            }
            uiElement.INTERNAL_RenderTransformOriginHasBeenApplied = true;
        }
    }
}
