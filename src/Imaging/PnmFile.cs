using FSpot.Imaging;
using SemWeb;
using System;
using System.IO;

#if ENABLE_NUNIT
using NUnit.Framework;
#endif

namespace FSpot.Pnm {
	public class PnmFile : ImageFile, IWritableImageFile, StatementSource {

                // false seems a safe default
                public bool Distinct {
                        get { return false; }
                }

		public PnmFile (Uri uri) : base (uri) 
		{
		}

		public PnmFile (string path) : base (path) 
		{
		}

		public class Header {
			public string Magic;
			public int Width;
			public int Height;
			public ushort Max;
			
			public Header (Stream stream)
			{
				Magic = GetString (stream);
				Width = int.Parse (GetString (stream));
				Height = int.Parse (GetString (stream));
				Max = ushort.Parse (GetString (stream));
			}

			public bool IsDeep {
				get {
					return Max > 256;
				}
			}

			public void Dump ()
			{
				System.Console.WriteLine ("Loading ({0} - {1},{2} - {3})", 
							  Magic, Width, Height, Max);
			}
		}

		public void Select (StatementSink sink)
		{
			using (Stream stream = Open ()) {
				Header header = new Header (stream);
				MetadataStore.AddLiteral (sink, "tiff:ImageWidth", header.Width.ToString ());
				MetadataStore.AddLiteral (sink, "tiff:ImageLength", header.Height.ToString ());
				string bits = header.IsDeep ? "16" : "8";
				MetadataStore.Add (sink, "tiff:BitsPerSample", "rdf:Seq", new string [] { bits, bits, bits });
			}
		}
		
		static char EatComment (Stream stream)
		{
			char c;
			do {
				c = (char)stream.ReadByte ();
				
			} while (c != '\n' && c != '\n');
			
			return c;
		}

		static string GetString (Stream stream)
		{
			System.Text.StringBuilder builder = new System.Text.StringBuilder ();

			char c;
			do {
				c = (char)stream.ReadByte ();
				if (c == '#')
					c = EatComment (stream);

			} while (char.IsWhiteSpace (c));
			
			while (! char.IsWhiteSpace (c)) {
				builder.Append (c);
				c = (char)stream.ReadByte ();				
			}
			
			return builder.ToString ();
		}

		public void Save (Gdk.Pixbuf pixbuf, System.IO.Stream stream)
		{
			if (pixbuf.HasAlpha)
				throw new NotImplementedException ();

			// FIXME this should be part of the header class
			string header = String.Format ("P6\n"
						       + "#Software: {0} {1}\n"
						       + "{2} {3}  #Width and Height\n"
						       + "255\n", 
						       FSpot.Defines.PACKAGE,
						       FSpot.Defines.VERSION,
						       pixbuf.Width, 
						       pixbuf.Height);
						       
			byte [] header_bytes = System.Text.Encoding.UTF8.GetBytes (header);
			stream.Write (header_bytes, 0, header.Length);
										 
			unsafe {
				byte * src_pixels = (byte *) pixbuf.Pixels;
				int src_stride = pixbuf.Rowstride;
				int count = pixbuf.Width * pixbuf.NChannels;
				int height = pixbuf.Height;

				for (int y = 0; y < height; y++) {
					for (int x = 0; x < count; x++) {
						stream.WriteByte (* (src_pixels + x));
					}
					src_pixels += src_stride;
				}
			}
		}
	}
}
