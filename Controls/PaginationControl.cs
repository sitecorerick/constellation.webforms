namespace Spark.WebForms.Controls
{
	using System;
	using System.Globalization;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.WebControls;

	using Spark.Html;
	using Spark.Web;
	using Spark.Web.Pagination;

	/// <summary>
	/// Represents the WebControl implementation of a PaginationControl.
	/// </summary>
	public class PaginationControl : WebControl, IPaginationControl
	{
		#region Construtors

		/// <summary>
		/// Initializes a new instance of the <see cref="PaginationControl"/> class.
		/// </summary>
		public PaginationControl()
		{
			this.MaxLinks = 13;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the <see cref="Paginator"/> control.
		/// </summary>
		public Paginator Paginator { get; set; }

		/// <summary>
		/// Gets or sets the maximum amount of page links to be displayed.
		/// </summary>
		public int MaxLinks { get; set; }

		/// <summary>
		/// Gets the current page the paginator is on.
		/// </summary>
		public int CurrentPage
		{
			get
			{
				int pageNumber;
				return int.TryParse(HttpContext.Current.Request.QueryString["page"], out pageNumber)
						   ? PageIndex(pageNumber)
						   : 0;
			}
		}

		/// <summary>
		/// Gets the current URL stripped of the page indicator querystring.
		/// </summary>
		public string BaseUrl
		{
			get
			{
				return UrlHelper.RemoveParameterFromUrl("page");
			}
		}

		#endregion

		/// <summary>
		/// Represents the method for rendering the paginator control.
		/// </summary>
		/// <param name="writer">The HtmlTextWriter for the request.</param>
		protected override void Render(HtmlTextWriter writer)
		{
			base.Render(writer);

			if (Paginator.PageCount < 2 || string.IsNullOrEmpty(this.BaseUrl))
			{
				return;
			}

			var currentPage = this.CurrentPage;
			this.RenderPaginator(currentPage, writer);
		}

		/// <summary>
		/// Retrieves the zero-based page index.
		/// </summary>
		/// <param name="pageNumber">The page number.</param>
		/// <returns>The page index.</returns>
		private static int PageIndex(int pageNumber)
		{
			int pageIndex = pageNumber - 1;
			return pageIndex > 0 ? pageIndex : 0;
		}

		/// <summary>
		/// Retrieves the page number.
		/// </summary>
		/// <param name="pageIndex">Zero-based page index.</param>
		/// <returns>The page number.</returns>
		private static int PageNumber(int pageIndex)
		{
			return pageIndex + 1;
		}

		/// <summary>
		/// Finds the upper and lower bounds of a window centered around a point.
		/// </summary>
		/// <param name="min">The lowest possible left.</param>
		/// <param name="max">The highest possible right.</param>
		/// <param name="center">Where the center of the window currently lies.</param>
		/// <param name="window">The total number of.</param>
		/// <param name="left">The calculated left value.</param>
		/// <param name="right">The calculated right value.</param>
		private static void SetBounds(int min, int max, int center, int window, out int left, out int right)
		{
			if (max - min <= window)
			{
				left = min;
				right = max;
				return;
			}

			left = center - (window / 2);
			right = center + (window / 2);

			if (left >= min && right <= max)
			{
				return;
			}

			if (left < min)
			{
				SetBounds(min, max, center + 1, window, out left, out right);
			}
			else if (right > max)
			{
				SetBounds(min, max, center - 1, window, out left, out right);
			}
		}

		/// <summary>
		/// Renders the "previous" link.
		/// </summary>
		/// <param name="currentPage">The page that should be rendered.</param>
		/// <param name="writer">The HtmlTextWriter for the request.</param>
		private void RenderPrev(int currentPage, HtmlTextWriter writer)
		{
			if (currentPage == 0)
			{
				return;
			}

			string url = this.GetPageUrl(currentPage - 1);

			using (writer.RenderLi())
			{
				using (writer.RenderA(url))
				{
					writer.Write("Prev");
				}
			}
		}

		/// <summary>
		/// Renders the "next" link.
		/// </summary>
		/// <param name="currentPage">The page that should be rendered.</param>
		/// <param name="writer">The HtmlTextWriter fro the request.</param>
		private void RenderNext(int currentPage, HtmlTextWriter writer)
		{
			if (currentPage == Paginator.PageCount - 1)
			{
				return;
			}

			var url = this.GetPageUrl(currentPage + 1);

			using (writer.RenderLi())
			{
				using (writer.RenderA(url))
				{
					writer.Write("Next");
				}
			}
		}

		/// <summary>
		/// Renders the paginator controls.
		/// </summary>
		/// <param name="currentPage">The current page to render.</param>
		/// <param name="writer">The <see cref="HtmlTextWriter"/> to render the controls through.</param>
		private void RenderPaginator(int currentPage, HtmlTextWriter writer)
		{
			using (writer.RenderDiv(cssClass: "pagination"))
			{
				using (writer.RenderUl())
				{
					this.RenderPrev(currentPage, writer);
					this.RenderPageLinks(currentPage, writer);
					this.RenderNext(currentPage, writer);
				}
			}
		}

		/// <summary>
		/// Renders the page links.
		/// </summary>
		/// <param name="currentPage">The current page to render.</param>
		/// <param name="writer">The <see cref="HtmlTextWriter"/> to render the controls through.</param>
		private void RenderPageLinks(int currentPage, HtmlTextWriter writer)
		{
			int left, right;
			SetBounds(1, Paginator.PageCount - 2, currentPage, this.MaxLinks - 2, out left, out right);

			for (var i = 0; i < Paginator.PageCount; i++)
			{
				if (!this.ShowLink(i, left, right))
				{
					if (i == left - 1 || i == right + 1)
					{
						using (writer.RenderLi(cssClass: "disabled"))
						{
							using (writer.RenderA("#"))
							{
								writer.Write("...");
							}
						}
					}

					continue;
				}

				var pageUrl = this.GetPageUrl(i);
				var pageNumber = PageNumber(i);
				var css = i == currentPage ? "active" : string.Empty;

				using (writer.RenderLi(cssClass: css))
				{
					using (writer.RenderA(pageUrl))
					{
						writer.Write(pageNumber.ToString(CultureInfo.InvariantCulture));
					}
				}
			}
		}

		/// <summary>
		/// Determines whether to show a link item or not.
		/// </summary>
		/// <param name="pageIndex">Index of the current page.</param>
		/// <param name="windowLeft">Window left value.</param>
		/// <param name="windowRight">Window right value.</param>
		/// <returns>Whether or not to show the link.</returns>
		private bool ShowLink(int pageIndex, int windowLeft, int windowRight)
		{
			if (pageIndex == 0 || pageIndex == Paginator.PageCount - 1)
			{
				return true;
			}

			if (pageIndex >= windowLeft && pageIndex <= windowRight)
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Retrieves the fully formatted url.
		/// </summary>
		/// <param name="pageIndex">Zero-based page index.</param>
		/// <returns>The url with page argument appended.</returns>
		private string GetPageUrl(int pageIndex)
		{
			var page = PageNumber(pageIndex);
			return pageIndex <= 0 ? this.BaseUrl : this.CreateUrl(page);
		}

		/// <summary>
		/// Creates a fully formatted url for use with building page links.
		/// </summary>
		/// <param name="page">The page number.</param>
		/// <returns>The fully formatted url.</returns>
		private string CreateUrl(int page)
		{
			var request = HttpContext.Current.Request;

			var helper = new QueryStringHelper(request.Url.Query);
			helper.AddOrReplace("page", page.ToString(CultureInfo.InvariantCulture));

			var returnUri = new UriBuilder(request.Url.Scheme, request.Url.Host, request.Url.Port)
			{
				Path = request.Url.AbsolutePath,
				Query = helper.GetQueryString()
			};

			return returnUri.ToString();
		}
	}
}
