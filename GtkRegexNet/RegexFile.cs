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
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace GtkRegexNet
{
	[Serializable]
	public struct RegexFile
	{
		public string Regex { get; set; }
		public string SearchInput { get; set; }
		public string ReplaceInput { get; set; }
		
		public static RegexFile Open(string Filename)
		{
			using(FileStream XmlFile = new FileStream(Filename,
			                                          FileMode.Open,
			                                          FileAccess.Read,
			                                          FileShare.Read)) {
				XmlSerializer XmlSer = new XmlSerializer(typeof(RegexFile));
				return (RegexFile)XmlSer.Deserialize(XmlFile);
			}
		}
		
		public void Save(string Filename)
		{
			using(FileStream XmlFile = new FileStream(Filename,
			                                          FileMode.Create,
			                                          FileAccess.Write,
			                                          FileShare.None)) {
				XmlSerializer XmlSer = new XmlSerializer(this.GetType());
				XmlSer.Serialize(XmlFile, this);
			}
		}
		
	}
}
