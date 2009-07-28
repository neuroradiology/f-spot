//
// Fspot/Loaders/ProgressHintEventArgs.cs
//
// Author(s)
//	Ruben Vermeersch  <ruben@savanne.be>
//
// This is free software. See COPYING for details
//

using System;

namespace FSpot.Loaders {
	public class ProgressHintEventArgs : EventArgs
	{
		public string Text { get; private set; }
		public double Fraction { get; private set; }

		public ProgressHintEventArgs (string text, double fraction) : base ()
		{
			this.Text = text;
			this.Fraction = fraction;
		}
	}
}
