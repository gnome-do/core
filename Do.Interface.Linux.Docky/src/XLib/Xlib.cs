// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2006 Novell, Inc. (http://www.novell.com)
//
//

using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;

namespace Docky.XLib {

	public enum PropertyMode
	{
		PropModeReplace = 0, 
		PropModePrepend = 1, 
		PropModeAppend = 2,
	}
	
	public enum Struts 
	{
		Left = 0,
		Right = 1,
		Top = 2,
		Bottom = 3,
		LeftStart = 4,
		LeftEnd = 5,
		RightStart = 6,
		RightEnd = 7,
		TopStart = 8,
		TopEnd = 9,
		BottomStart = 10,
		BottomEnd = 11
	}
	
	internal class Xlib {
		const string libX11 = "X11";
		const string libGdkX11 = "libgdk-x11";
		
		[DllImport (libGdkX11)]
		static extern IntPtr gdk_x11_drawable_get_xid (IntPtr handle);
		
		[DllImport (libGdkX11)]
		static extern IntPtr gdk_x11_drawable_get_xdisplay (IntPtr handle);

		public static IntPtr GdkWindowX11Xid (Gdk.Window window)
		{
			return gdk_x11_drawable_get_xid (window.Handle);
		}

		public static IntPtr GdkDrawableXDisplay (Gdk.Window window)
		{
			return gdk_x11_drawable_get_xdisplay (window.Handle);
		}
		
		[DllImport (libX11)]
		public extern static IntPtr XOpenDisplay (IntPtr display);
		
		[DllImport (libX11)]
		public extern static int XInternAtoms (IntPtr display, string[] atom_names, int atom_count, bool only_if_exists, IntPtr[] atoms);
		
		[DllImport (libX11)]
		extern static int XChangeProperty (IntPtr display, IntPtr window, IntPtr property, IntPtr type, int format, int mode, IntPtr[] data, int nelements);
	
		public static int XChangeProperty (Gdk.Window window, IntPtr property, IntPtr type, int mode, uint[] data)
		{
			IntPtr [] dataArray = data.Select (i => (IntPtr) i).ToArray ();
			return XChangeProperty (GdkDrawableXDisplay (window), GdkWindowX11Xid (window), property, type, 32, mode, dataArray, data.Length); 
		}
	}
}
