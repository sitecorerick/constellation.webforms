namespace Spark.WebForms
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.UI;

	/// <summary>
	/// Set of extensions for interacting with controls.
	/// </summary>
	public static class ControlExtensions
	{
		/// <summary>
		/// Finds all controls of a type.
		/// </summary>
		/// <typeparam name="T">The type of control.</typeparam>
		/// <param name="startingPoint">The container.</param>
		/// <returns>The controls of the specified type.</returns>
		public static IEnumerable<T> AllControls<T>(this Control startingPoint)
			where T : Control
		{
			var hit = startingPoint is T;
			if (hit)
			{
				yield return startingPoint as T;
			}

			foreach (var child in startingPoint.Controls.Cast<Control>())
			{
				foreach (var control in AllControls<T>(child))
				{
					yield return control;
				}
			}
		}
	}
}