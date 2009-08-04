//
// FSpot.Editors.EditorNode.cs
//
// Author(s)
// 	Ruben Vermeersch <ruben@savanne.be>
//
// This is free software. See COPYING for details.
//

using Mono.Addins;

namespace FSpot.Editors {
	[ExtensionNode ("Editor")]
	public class EditorNode : ExtensionNode {
		[NodeAttribute (Required=true)]
		protected string editor_type;

		public Editor GetEditor () {
			return (Editor) Addin.CreateInstance (editor_type);
		}
	}
}
