// Copyright (c) 2011, TheWarrentTeam
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Text.RegularExpressions;
using Gtk;

public partial class MainWindow : Gtk.Window
{
	private Regex reg_CurrentRegex;
	private RegexOptions regopt_CurrentOptions;
	private TreeViewColumn tvc_MatchesView;
	private GtkRegexNet.SettingFile sf_Settings;
	
	public MainWindow () : base(Gtk.WindowType.Toplevel)
	{
		Build ();
		
		this.OpenSetting();
		
		TextTag MatchTag;
		
		MatchTag = new TextTag("MatchTag");
		MatchTag.ForegroundGdk = new Gdk.Color(0, 200, 0);
		//MatchTag.Underline = Pango.Underline.Single;
		this.MatchResultTextView.Buffer.TagTable.Add(MatchTag);
		
		MatchTag = new TextTag("MatchTag");
		MatchTag.ForegroundGdk = new Gdk.Color(0, 200, 0);
		//MatchTag.Underline = Pango.Underline.Single;
		this.ReplaceResultTextView.Buffer.TagTable.Add(MatchTag);
		
		//Text Changed events
		this.RegexTextView.Buffer.Changed += this.OnRegexTextBufferChangedEvent;
		this.SearchInputTextView.Buffer.Changed += this.OnSearchStringTextBufferChangedEvent;
		this.ReplaceInputTextView.Buffer.Changed += this.OnReplaceStringTextBufferChangedEvent;
	}

	protected void OnRegexTextBufferChangedEvent (object sender, EventArgs e)
	{
		this.UpdateRegexObject();
	}
	
	protected void OnSearchStringTextBufferChangedEvent (object sender, EventArgs e)
	{
		this.UpdateRegexSearch ();
	}
	
	protected void OnReplaceStringTextBufferChangedEvent (object sender, EventArgs e)
	{
		this.UpdateRegexSearch ();
	}
	
	protected virtual void IgnoreCaseToggledEvent (object sender, System.EventArgs e)
	{
		if(this.IgnoreCaseCheckButton.Active)
			this.regopt_CurrentOptions |= RegexOptions.IgnoreCase;
		else 
			this.regopt_CurrentOptions &= ~RegexOptions.IgnoreCase;
		
		this.UpdateRegexObject();
	}
	
	protected virtual void IgnorePatternWhitespaceToggledEvent (object sender, System.EventArgs e)
	{
		if(this.IgnoreCaseCheckButton.Active)
			this.regopt_CurrentOptions |= RegexOptions.IgnoreCase;
		else 
			this.regopt_CurrentOptions &= ~RegexOptions.IgnoreCase;
		
		this.UpdateRegexObject();
	}
	
	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}
	
	private void UpdateRegexObject()
	{
		if(this.RegexTextView.Buffer.Text.Length <= 0) {
			this.RegexStatusLabel.Text = "Regex: Empty";
			this.reg_CurrentRegex = null;
			return;
		}
		
		try {
			this.reg_CurrentRegex = new Regex(this.RegexTextView.Buffer.Text, this.regopt_CurrentOptions);
			this.RegexStatusLabel.Text = "Regex: Ok";
		} catch (Exception ex) {
			this.reg_CurrentRegex = null;
			this.RegexStatusLabel.Text = string.Format("Regex: {0}", ex.Message);
		}
		this.UpdateRegexSearch();
	}
	
	private void UpdateRegexSearch()
	{
		if(this.tvc_MatchesView != null) {
			this.GroupResultTreeView.RemoveColumn(this.tvc_MatchesView);
			this.tvc_MatchesView.Destroy();
			this.tvc_MatchesView.Dispose();
		}
		
		if(this.reg_CurrentRegex == null)
			return;
		
		//Init Group Result TreeView
		CellRendererText TextCell = new CellRendererText();
		this.tvc_MatchesView = new TreeViewColumn();
		
		this.tvc_MatchesView.PackStart(TextCell, true);
		this.tvc_MatchesView.AddAttribute(TextCell, "text", 0);
		
		TreeStore Store = new TreeStore(typeof(string));
		
		//Init Match Result TextView
		this.MatchResultTextView.Buffer.Text = this.SearchInputTextView.Buffer.Text;
		
		//Replace TextView
		this.ReplaceResultTextView.Buffer.Text = this.reg_CurrentRegex.Replace(this.SearchInputTextView.Buffer.Text,
		                                                                       this.ReplaceInputTextView.Buffer.Text);
		if(this.ReplaceInputTextView.Buffer.Text.Length > 0 &&
		   this.ReplaceResultTextView.Buffer.Text.Length > 0) {
			
			int StartIndex = 0;
			int Index = 0;
			int ReplaceLength = this.ReplaceInputTextView.Buffer.Text.Length;
			while ((Index = this.ReplaceResultTextView.Buffer.Text.IndexOf(this.ReplaceInputTextView.Buffer.Text, StartIndex)) >= 0) {
				TextIter IndexIterStart = this.ReplaceResultTextView.Buffer.GetIterAtOffset(Index);
				TextIter IndexIterEnd = this.ReplaceResultTextView.Buffer.GetIterAtOffset(Index + ReplaceLength);
				this.ReplaceResultTextView.Buffer.ApplyTag("MatchTag", IndexIterStart, IndexIterEnd);
				
				StartIndex = Index + ReplaceLength;
				if(StartIndex > this.ReplaceResultTextView.Buffer.Text.Length)
					break;
			}
		}
		
		//Apply matches
		MatchCollection Matches = this.reg_CurrentRegex.Matches(this.SearchInputTextView.Buffer.Text);
		foreach (Match MatchItem in Matches) {
			if(!MatchItem.Success)
				continue;
			
			//TreeView Add Match node.
			TreeIter Iter = Store.AppendValues(MatchItem.ToString());
			this.TreeAddMatch(Store, Iter, MatchItem);
			
			//Match TextView add color tag.
			TextIter TxtIterStart = this.MatchResultTextView.Buffer.GetIterAtOffset(MatchItem.Index);
			TextIter TxtIterEnd = this.MatchResultTextView.Buffer.GetIterAtOffset(MatchItem.Index + MatchItem.Length);
			this.MatchResultTextView.Buffer.ApplyTag("MatchTag", TxtIterStart, TxtIterEnd);
		}
		
		this.GroupResultTreeView.AppendColumn(this.tvc_MatchesView);
		this.GroupResultTreeView.Model = Store;
	}
	
	private void TreeAddMatch(TreeStore store, TreeIter iter, Match match)
	{
		foreach (Group GroupItem in match.Groups) {
			if(!GroupItem.Success)
				continue;
			
			TreeIter GroupIter = store.AppendValues(iter, GroupItem.ToString());
			
			foreach (Capture CaptureItem in GroupItem.Captures) {
				store.AppendValues(GroupIter, CaptureItem.ToString());
			}
		}
	}
	
	protected virtual void OpenActivatedEvent (object sender, System.EventArgs e)
	{
		using (Gtk.FileChooserDialog OpenFile = new Gtk.FileChooserDialog("Regex Open",
		                                                                  null,
		                                                                  Gtk.FileChooserAction.Open,
		                                                                  Gtk.Stock.Open, Gtk.ResponseType.Accept,
		                                                                  Gtk.Stock.Cancel, Gtk.ResponseType.Cancel
		                                                                  )){
			OpenFile.SetCurrentFolder(this.sf_Settings.WorkFolder);
			
			OpenFile.Filter = new Gtk.FileFilter();
			OpenFile.Filter.AddPattern("*.grn");
			
			if(OpenFile.Run() == (int)Gtk.ResponseType.Accept) {
				this.LastOpenDirSetting(OpenFile.Filename.Substring(0, OpenFile.Filename.LastIndexOfAny(new char[] {'/', '\\'})));
				
				GtkRegexNet.RegexFile GRNFile = GtkRegexNet.RegexFile.Open(OpenFile.Filename);
				this.RegexTextView.Buffer.Text = GRNFile.Regex;
				this.SearchInputTextView.Buffer.Text = GRNFile.SearchInput;
				this.ReplaceInputTextView.Buffer.Text = GRNFile.ReplaceInput;
			}
			
			OpenFile.Destroy();
		}
	}
	
	protected virtual void SaveAsActivatedEvent (object sender, System.EventArgs e)
	{
		using (Gtk.FileChooserDialog SaveFile = new Gtk.FileChooserDialog("Regex SaveAs",
		                                                                  null,
		                                                                  Gtk.FileChooserAction.Save,
		                                                                  Gtk.Stock.SaveAs, Gtk.ResponseType.Accept,
		                                                                  Gtk.Stock.Cancel, Gtk.ResponseType.Cancel
		                                                                  )){
			SaveFile.SetCurrentFolder(this.sf_Settings.WorkFolder);
			
			SaveFile.Filter = new Gtk.FileFilter();
			SaveFile.Filter.AddPattern("*.grn");
			
			if(SaveFile.Run() == (int)Gtk.ResponseType.Accept) {
				this.LastOpenDirSetting(SaveFile.Filename.Substring(0, SaveFile.Filename.LastIndexOfAny(new char[] {'/', '\\'})));
				
				GtkRegexNet.RegexFile GRNFile = new GtkRegexNet.RegexFile();
				GRNFile.Regex = this.RegexTextView.Buffer.Text;
				GRNFile.SearchInput = this.SearchInputTextView.Buffer.Text;
				GRNFile.ReplaceInput = this.ReplaceInputTextView.Buffer.Text;
				GRNFile.Save(SaveFile.Filename);
			}
			
			SaveFile.Destroy();
		}
	}
	
	private void OpenSetting()
	{
		string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		string FolderSettingPath = AppDataPath + "/GtkRegexNet";
		
		if(!System.IO.Directory.Exists(FolderSettingPath))
			System.IO.Directory.CreateDirectory(FolderSettingPath);
		
		string SettingPath = FolderSettingPath + "/Setting.Xml";
		
		if(!System.IO.File.Exists(SettingPath)) {
			this.sf_Settings = new GtkRegexNet.SettingFile();
		} else
			this.sf_Settings = GtkRegexNet.SettingFile.Open(SettingPath);
		
		if(this.sf_Settings.WorkFolder ==  string.Empty) {
			this.sf_Settings.WorkFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			this.sf_Settings.Save(SettingPath);
		}
	}
	
	private void LastOpenDirSetting(string LastPath)
	{
		string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		string SettingPath = AppDataPath + "/GtkRegexNet/Setting.Xml";
		this.sf_Settings.WorkFolder = LastPath;
		this.sf_Settings.Save(SettingPath);
	}
	
		protected virtual void AboutActivatedEvent (object sender, System.EventArgs e)
	{
		AboutDialog AboutBox = new AboutDialog();
		AboutBox.ProgramName = "GtkRegexNet";
		AboutBox.Copyright = "Copyright (c) 2011\nAll rights reserved.";
		AboutBox.License = @"Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:
    * Redistributions of source code must retain the above copyright
      notice, this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above copyright
      notice, this list of conditions and the following disclaimer in the
      documentation and/or other materials provided with the distribution.
    * Neither the name of The Warrent Team nor the
      names of its contributors may be used to endorse or promote products
      derived from this software without specific prior written permission.
    * This software is free for non-commercial use. You may not use this
      software, in whole or in part, in support of any commercial product
      without the express consent of the author.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS ""AS IS"" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE WARRENT TEAM BE LIABLE FOR ANY
DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.";
		AboutBox.Authors = new string[] {
			"Marc-Andre Ferland - Madrang"
		};
		AboutBox.Website = @"http://sites.google.com/site/warrentteam/";
		
		AboutBox.Run();
		AboutBox.Hide();
		AboutBox.Dispose();
	}
	
}
