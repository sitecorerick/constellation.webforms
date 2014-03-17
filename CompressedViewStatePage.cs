namespace Constellation.WebForms
{
	using System;
	using System.Globalization;
	using System.IO;
	using System.Web.UI;
	using ICSharpCode.SharpZipLib.Zip.Compression;
	using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

	/// <summary>
	/// A Page object that compresses ViewState to reduce the size of the 
	/// ViewState input element on page.
	/// </summary>
	public class CompressedViewStatePage : Page
	{
		/// <summary>
		/// The x64 buffer size for the compression engine.
		/// </summary>
		private const int Buffersize = 65536;

		/// <summary>
		/// The default compression level.
		/// </summary>
		private int viewStateCompression = Deflater.BEST_SPEED;

		/// <summary>
		/// Gets or sets the compression level of the page instance.
		/// </summary>
		public int ViewStateCompression
		{
			get { return this.viewStateCompression; }
			set { this.viewStateCompression = value; }
		}

		/// <summary>
		/// Override of the default page behavior allowing us to compress
		/// ViewState before writing it to the page.
		/// </summary>
		/// <param name="state">The state bag.</param>
		protected override void SavePageStateToPersistenceMedium(object state)
		{
			if (this.ViewStateCompression == Deflater.NO_COMPRESSION)
			{
				base.SavePageStateToPersistenceMedium(state);
				return;
			}

			var viewState = state;

			// Strip controlstate if necessary.
			var pair = state as Pair;
			if (pair != null)
			{
				// Sometimes the state object contains ControlState and ViewState, this handles that case.
				PageStatePersister.ControlState = pair.First;
				viewState = pair.Second;
			}

			using (var writer = new StringWriter(CultureInfo.InvariantCulture))
			{
				new LosFormatter().Serialize(writer, viewState);
				var base64 = writer.ToString();
				var compressed = Compress(Convert.FromBase64String(base64), this.ViewStateCompression);
				PageStatePersister.ViewState = Convert.ToBase64String(compressed);
			}

			PageStatePersister.Save();
		}

		/// <summary>
		/// Override of the default page behavior allows us to decompress
		/// ViewState before the page processes it.
		/// </summary>
		/// <returns>The state bag.</returns>
		protected override object LoadPageStateFromPersistenceMedium()
		{
			if (this.viewStateCompression == Deflater.NO_COMPRESSION)
			{
				return base.LoadPageStateFromPersistenceMedium();
			}

			PageStatePersister.Load();
			var base64 = PageStatePersister.ViewState.ToString();
			var state = Decompress(Convert.FromBase64String(base64));
			var serializedState = Convert.ToBase64String(state);

			var viewState = new LosFormatter().Deserialize(serializedState);
			return new Pair(PageStatePersister.ControlState, viewState);
		}

		/// <summary>
		/// The compression routine.
		/// </summary>
		/// <param name="bytes">The bytes to compress.</param>
		/// <param name="compressionLevel">The compression algorithm to apply to the byte array.</param>
		/// <returns>A compressed array of bytes.</returns>
		private static byte[] Compress(byte[] bytes, int compressionLevel)
		{
			using (var memoryStream = new MemoryStream(Buffersize))
			{
				var deflater = new Deflater(compressionLevel);
				using (Stream stream = new DeflaterOutputStream(memoryStream, deflater, Buffersize))
				{
					stream.Write(bytes, 0, bytes.Length);
				}

				return memoryStream.ToArray();
			}
		}

		/// <summary>
		/// The decompression routine.
		/// </summary>
		/// <param name="bytes">The bytes to decompress.</param>
		/// <returns>A decompressed array of bytes.</returns>
		private static byte[] Decompress(byte[] bytes)
		{
			using (var byteStream = new MemoryStream(bytes))
			{
				using (Stream stream = new InflaterInputStream(byteStream))
				{
					using (var memory = new MemoryStream(Buffersize))
					{
						var buffer = new byte[Buffersize];
						while (true)
						{
							var size = stream.Read(buffer, 0, Buffersize);
							if (size <= 0)
							{
								break;
							}

							memory.Write(buffer, 0, size);
						}

						return memory.ToArray();
					}
				}
			}
		}
	}
}
