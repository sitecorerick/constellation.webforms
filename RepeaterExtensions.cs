namespace Spark.WebForms
{
	using System.Web.UI.WebControls;

	/// <summary>
	/// Contains helper extensions for working with ASP.NET Repeater Controls.
	/// </summary>
	public static class RepeaterExtensions
	{
		/// <summary>
		/// Checks if the current repeater item is <see cref="ListItemType.Item"/> or <see cref="ListItemType.AlternatingItem"/>.
		/// </summary>
		/// <param name="e">The event args that contains the Repeater item.</param>
		/// <returns>True or false.</returns>
		public static bool IsDataItem(this RepeaterItemEventArgs e)
		{
			if (e == null)
			{
				return false;
			}

			var itemType = e.Item.ItemType;
			return itemType == ListItemType.Item || itemType == ListItemType.AlternatingItem;
		}

		/// <summary>
		/// Tests if the repeater item is a data item, if so it tries to cast it to the specified type.
		/// </summary>
		/// <typeparam name="T">The type the data item should be.</typeparam>
		/// <param name="e">The event args containing the repeater item.</param>
		/// <param name="dataItem">The dataItem to return.</param>
		/// <returns>True if item is a data item and casts successfully, otherwise false.</returns>
		public static bool TryGetDataItem<T>(this RepeaterItemEventArgs e, out T dataItem)
		{
			dataItem = default(T);

			if (!e.IsDataItem() || e.Item.DataItem == null)
			{
				return false;
			}

			var itemType = e.Item.DataItem.GetType();
			if (!typeof(T).IsAssignableFrom(itemType))
			{
				return false;
			}

			dataItem = (T)e.Item.DataItem;
			return true;
		}
	}
}
