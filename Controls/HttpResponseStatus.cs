namespace Constellation.WebForms.Controls
{
	using System.Web.UI.WebControls;

	/// <summary>
	/// Sets the status code of the response. Default behavior is "200 OK".
	/// </summary>
	public class HttpResponseStatus : WebControl
	{
		/// <summary>
		/// Internal storage of the integer value.
		/// </summary>
		private int statusCode = 200;

		/// <summary>
		/// Internal storage of the description.
		/// </summary>
		private string statusDescription = "OK";

		/// <summary>
		/// Gets or sets the Http 1.1 Status code.
		/// </summary>
		public int StatusCode
		{
			get { return this.statusCode; }
			set { this.statusCode = value; }
		}

		/// <summary>
		/// Gets or sets the Http 1.1 Status description.
		/// </summary>
		public string StatusDescription
		{
			get { return this.statusDescription; }
			set { this.statusDescription = value; }
		}

		/// <summary>
		/// Contract for  WebControl.
		/// </summary>
		/// <param name="output">The Response's HtmlTextWriter.</param>
		protected override void Render(System.Web.UI.HtmlTextWriter output)
		{
			this.Page.Response.StatusCode = this.statusCode;
			this.Page.Response.StatusDescription = this.statusDescription;
		}
	}
}
