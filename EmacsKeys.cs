using System.Collections.Generic;
using System;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Core;
using Mono.TextEditor;

namespace Nsf.EmacsKeys {
	internal enum Action {
		ThirdParty,
		KillWord,
		KillLine,
		Other,
	}

	internal class State {
		internal static Dictionary<Document, State> Table = new Dictionary<Document, State>();

		internal static State GetOrCreateFor(Document doc) {
			State state;
			if (!Table.TryGetValue(doc, out state)) {
				state = new State();
				state.Editor = doc.Editor;
				Table[doc] = state;
			}
				
			return state;
		}

		internal int Mark = -1;
		internal Action LastAction = Action.Other;
		internal bool OwnAction = false;
		internal TextEditorData Editor = null;

		// This function is triggered by various events which lead to modification of the text editor state: selection, content, caret position.
		internal void OnThirdPartyAction() {
			if (OwnAction)
				return;
			
			if (Mark != -1) {
				using (var own = new OwnAction(this, Action.ThirdParty)) {
					Editor.ClearSelection();
					Mark = -1;
				}
			} else {
				LastAction = Action.ThirdParty;
			}		
		}
	}

	internal class OwnAction : IDisposable {
		internal State State;
		internal Action Action;

		internal OwnAction(State state, Action action) {
			State = state;
			Action = action;

			State.OwnAction = true;
		}

		public void Dispose() {
			State.OwnAction = false;
			State.LastAction = Action;
		}
	}

	internal static class Utils {
		internal static void Move(Action<TextEditorData> a) {
			State state = State;
			if (state == null)
				return;

			using (var own = new OwnAction(state, Action.Other)) {
				if (state.Mark != -1)
					SelectionActions.Select(state.Editor, a);
				else
					a(state.Editor);
			}
		}

		internal static State State {
			get {
				var doc = IdeApp.Workbench.ActiveDocument;
				return (doc != null && doc.Editor != null) ? State.GetOrCreateFor(doc) : null;
			}
		}
	}

	public class StartupHandler : CommandHandler {
		protected override void Run() {
			IdeApp.Workbench.ActiveDocumentChanged += (object sender, EventArgs e) => {
				var doc = IdeApp.Workbench.ActiveDocument;
				if (doc == null)
					return;

				if (!State.Table.ContainsKey(doc)) {
					var editor = doc.Editor;
					if (editor == null)
						return;

					State state = State.GetOrCreateFor(doc);
					editor.SelectionChanged += (object sender0, EventArgs e0) => {
						state.OnThirdPartyAction();
					};
					editor.Caret.PositionChanged += (object sender1, DocumentLocationEventArgs e1) => {
						state.OnThirdPartyAction();
					};
					editor.Document.TextReplaced += (object sender2, DocumentChangeEventArgs e2) => {
						state.OnThirdPartyAction();
					};
				}
			};
			IdeApp.Workbench.DocumentClosing += (object sender, MonoDevelop.Ide.Gui.DocumentEventArgs e) => {
				State.Table.Remove(e.Document);
			};
		}
	}

	public class MarkHandler : CommandHandler {
		protected override void Run() {
			State state = Utils.State;
			if (state == null)
				return;

			using (var own = new OwnAction(state, Action.Other)) {
				if (state.Mark == state.Editor.Caret.Offset) {
					state.Mark = -1;
				} else {
					state.Editor.ClearSelection();
					state.Mark = state.Editor.Caret.Offset;
				}
			}
		}
	}

	public class GotoNextCharHandler : CommandHandler {
		protected override void Run() {
			Utils.Move(CaretMoveActions.Right);
		}
	}

	public class GotoPrevCharHandler : CommandHandler {
		protected override void Run() {
			Utils.Move(CaretMoveActions.Left);
		}
	}

	public class GotoNextWordHandler : CommandHandler {
		protected override void Run() {
			Utils.Move(CaretMoveActions.NextWord);
		}
	}

	public class GotoPrevWordHandler : CommandHandler {
		protected override void Run() {
			Utils.Move(CaretMoveActions.PreviousWord);
		}
	}

	public class GotoNextSubwordHandler : CommandHandler {
		protected override void Run() {
			Utils.Move(CaretMoveActions.NextSubword);
		}
	}

	public class GotoPrevSubwordHandler : CommandHandler {
		protected override void Run() {
			Utils.Move(CaretMoveActions.PreviousSubword);
		}
	}

	public class GotoNextLineHandler : CommandHandler {
		protected override void Run() {
			Utils.Move(CaretMoveActions.Down);
		}
	}

	public class GotoPrevLineHandler : CommandHandler {
		protected override void Run() {
			Utils.Move(CaretMoveActions.Up);
		}
	}

	public class GotoLineStartHandler : CommandHandler {
		protected override void Run() {
			Utils.Move(CaretMoveActions.LineStart);
		}
	}

	public class GotoLineEndHandler : CommandHandler {
		protected override void Run() {
			Utils.Move(CaretMoveActions.LineEnd);
		}
	}

	public class GotoFileStartHandler : CommandHandler {
		protected override void Run() {
			Utils.Move(CaretMoveActions.ToDocumentStart);
		}
	}

	public class GotoFileEndHandler : CommandHandler {
		protected override void Run() {
			Utils.Move(CaretMoveActions.ToDocumentEnd);
		}
	}

	public class CopyHandler : CommandHandler {
		protected override void Run() {
			State state = Utils.State;
			if (state == null)
				return;
			using (var own = new OwnAction(state, Action.Other)) {
				ClipboardActions.Copy(state.Editor);
				state.Editor.ClearSelection();
				state.Mark = -1;
			}
		}
	}

	public enum Commands {
		Mark,
		GotoNextChar,
		GotoPrevChar,
		GotoNextWord,
		GotoPrevWord,
		GotoNextSubword,
		GotoPrevSubword,
		GotoNextLine,
		GotoPrevLine,
		GotoLineStart,
		GotoLineEnd,
		GotoFileStart,
		GotoFileEnd,

		Copy,
	}
}
