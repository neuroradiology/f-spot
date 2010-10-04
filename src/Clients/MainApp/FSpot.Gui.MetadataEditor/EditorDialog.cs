/*
 * EditorDialog.cs
 *
 * Author(s):
 *  Mike Gemuende <mike@gemuende.de>
 *
 * This is frees software. See COPYING for details.
 */

using System;
using System.Collections.Generic;
using System.Threading;

using Mono.Unix;

using Gtk;

using Hyena;
using Hyena.Widgets;

using FSpot.Widgets;
using FSpot.Core;
using FSpot.UI.Dialog;


namespace FSpot.Gui.MetadataEditor
{
    public class EditorDialog : BuilderDialog
    {

#region Private Fields

        [GtkBeans.Builder.Object] private HBox dialog_content;
        [GtkBeans.Builder.Object] private Alignment photo_view_alignment;
        [GtkBeans.Builder.Object] private VBox content_box;
        [GtkBeans.Builder.Object] private VBox progressbar_placeholder;

        [GtkBeans.Builder.Object] private VBox dialog_vbox;
        [GtkBeans.Builder.Object] private Button save_button;
        [GtkBeans.Builder.Object] private Button cancel_button;

        private AnimatedVBox progressbar_box;
        private ProgressBar progressbar;
        private Notebook editor_notebook;

        private Button move_next_button;
        private Button move_prev_button;

        private BrowseablePointerGridView photo_view;
        private EditorController controller;
        private bool loaded = false;
        private IPhoto selected_photo;

#endregion

#region Constructors

        public EditorDialog (IBrowsableCollection collection)
            : base ("MetadataEditorDialog.ui", "metadata_editor_dialog")
        {
            save_button.Clicked += (o, e) => { SaveContent (); };
            cancel_button.Clicked += (o, e) => { Destroy (); };

            // setup editor controller
            controller = new EditorController (collection);
            controller.Pointer.Changed += (o, e) => { UpdateMoveButtons (); };

            // setup photo view
            photo_view = new BrowseablePointerGridView (controller.Pointer) { MaxColumns = 1 };

            var scrolled_window = new Hyena.Widgets.ScrolledWindow ();
            photo_view_alignment.Add (scrolled_window);
            scrolled_window.AddWithFrame (photo_view);
            scrolled_window.WidthRequest = 200;

            photo_view_alignment.ShowAll ();

            // setup progressbar
            progressbar = new ProgressBar ();
            progressbar_box = new AnimatedVBox ();
            progressbar_placeholder.Add (progressbar_box);

            // setup editor notebook
            editor_notebook = new Notebook ();
            content_box.Add (editor_notebook);
            editor_notebook.Show ();

            AddEditorPage (new BasicEditorPage (controller));

            // setup navigation buttons
            move_prev_button = new Button (Stock.GoBack);
            move_next_button = new Button (Stock.GoForward);

            move_prev_button.Clicked += (o, e) => {
                controller.Pointer.MovePrevious ();
                photo_view.ScrollTo (controller.Pointer.Index);
            };
            move_next_button.Clicked += (o, e) => {
                controller.Pointer.MoveNext ();
                photo_view.ScrollTo (controller.Pointer.Index);
            };

            ActionArea.PackStart (move_prev_button, false, false, 0);
            ActionArea.PackStart (move_next_button, false, false, 0);
            ActionArea.SetChildSecondary (move_prev_button, true);
            ActionArea.SetChildSecondary (move_next_button, true);

            move_prev_button.Show ();
            move_next_button.Show ();

            progressbar_box.ShowAll ();
            content_box.ShowAll ();
            dialog_content.ShowAll ();
        }

#endregion

#region GUI Utility Methods

        private void EnableProgress ()
        {
            if ( ! progressbar_box.Contains (progressbar)) {

                if (progressbar.Parent != null)
                    (progressbar.Parent as Container).Remove (progressbar);

                progressbar.Show ();
                progressbar_box.Add (progressbar);
            }

            dialog_content.Sensitive = false;
            save_button.Sensitive = false;
            move_next_button.Sensitive = false;
            move_prev_button.Sensitive = false;
        }

        private void DisableProgress ()
        {
            dialog_content.Sensitive = true;
            save_button.Sensitive = true;

            // here we set the sensitivity of the move buttons according to
            // the selected photo instead of making them just sensitive
            UpdateMoveButtons ();

            if (progressbar_box.Contains (progressbar))
                progressbar_box.Remove (progressbar);
        }

        private void UpdateProgress (string text, double fraction)
        {
            progressbar.Text = text;
            progressbar.Fraction = fraction;
        }

        private void UpdateMoveButtons ()
        {
            move_prev_button.Sensitive = controller.Pointer.Index > 0;
            move_next_button.Sensitive = controller.Pointer.Index < controller.Pointer.Collection.Count - 1;
        }

#endregion

#region Public Methods

        public new void Run ()
        {
            LoadContent ();
            base.Run ();
        }

        public void AddEditorPage (EditorPage page)
        {
            var alignment = new Alignment (0.5f, 0.5f, 1.0f, 1.0f) {
                LeftPadding = 8,
                RightPadding = 8,
                TopPadding = 8,
                BottomPadding = 8};
            alignment.Add (page);

            editor_notebook.AppendPage (alignment, new Label (page.Title));

            editor_notebook.ShowTabs = (editor_notebook.NPages > 1);
        }

#endregion

#region Override Base Class Behavior

        protected override void OnDestroyed ()
        {
            base.OnDestroyed ();

            // ensure that running background threads are stopped
            stop_requested = true;
        }

#endregion

#region Background Working Stuff

        // is set to request a stop of background threads
        private bool stop_requested;

        private void LoadContent ()
        {
            UpdateProgress (String.Format (Catalog.GetString ("Photo {0} of {1} Loaded"), 0, controller.Collection.Count), 0);
            EnableProgress ();

            stop_requested = false;

            // start thread to load photos
            ThreadAssist.Spawn (() => {

                try {
                    controller.Load ((loaded_index, count) => {

                        ThreadAssist.ProxyToMain (() => UpdateProgress (String.Format (Catalog.GetString ("Photo {0} of {1} Loaded"),
                                                       loaded_index + 1, count),
                                        loaded_index / (double) count));

                        // request controller to stop, if neccesary
                        return ! stop_requested;

                    });
                } catch (Exception e) {
                    Log.DebugException (e);

                    if ( ! stop_requested)
                        ThreadAssist.ProxyToMain (Destroy);
                }

                if (stop_requested)
                    return;

                ThreadAssist.ProxyToMain (() => {
                    DisableProgress ();
                    controller.Pointer.MoveFirst ();
                });

            });
        }

        private void SaveContent ()
        {
            UpdateProgress (String.Format (Catalog.GetString ("Photo {0} of {1} Saved"), 0, controller.Collection.Count), 0);
            EnableProgress ();

            stop_requested = false;

            // start thread to save photos
            ThreadAssist.Spawn (() => {

                try {
                    controller.Store ((stored_index, count) => {

                        ThreadAssist.ProxyToMain (() => UpdateProgress (String.Format (Catalog.GetString ("Photo {0} of {1} Saved"),
                                                       stored_index + 1, count),
                                        stored_index / (double) count));

                        // request controller to stop, if neccesary
                        return ! stop_requested;

                    });
                } catch (Exception e) {
                    Log.DebugException (e);

                    if ( ! stop_requested)
                        ThreadAssist.ProxyToMain (Destroy);
                }

                if (stop_requested)
                    return;

                ThreadAssist.ProxyToMain (Destroy);
            });
        }

#endregion

    }
}

