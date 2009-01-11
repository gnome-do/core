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

namespace Docky.XLib {

	internal class X11Atoms {

		// Our atoms
		public readonly int AnyPropertyType		= (int)0;
		public readonly int XA_PRIMARY		= (int)1;
		public readonly int XA_SECONDARY		= (int)2;
		public readonly int XA_ARC			= (int)3;
		public readonly int XA_ATOM			= (int)4;
		public readonly int XA_BITMAP		= (int)5;
		public readonly int XA_CARDINAL		= (int)6;
		public readonly int XA_COLORMAP		= (int)7;
		public readonly int XA_CURSOR		= (int)8;
		public readonly int XA_CUT_BUFFER0		= (int)9;
		public readonly int XA_CUT_BUFFER1		= (int)10;
		public readonly int XA_CUT_BUFFER2		= (int)11;
		public readonly int XA_CUT_BUFFER3		= (int)12;
		public readonly int XA_CUT_BUFFER4		= (int)13;
		public readonly int XA_CUT_BUFFER5		= (int)14;
		public readonly int XA_CUT_BUFFER6		= (int)15;
		public readonly int XA_CUT_BUFFER7		= (int)16;
		public readonly int XA_DRAWABLE		= (int)17;
		public readonly int XA_FONT			= (int)18;
		public readonly int XA_INTEGER		= (int)19;
		public readonly int XA_PIXMAP		= (int)20;
		public readonly int XA_POINT			= (int)21;
		public readonly int XA_RECTANGLE		= (int)22;
		public readonly int XA_RESOURCE_MANAGER	= (int)23;
		public readonly int XA_RGB_COLOR_MAP		= (int)24;
		public readonly int XA_RGB_BEST_MAP		= (int)25;
		public readonly int XA_RGB_BLUE_MAP		= (int)26;
		public readonly int XA_RGB_DEFAULT_MAP	= (int)27;
		public readonly int XA_RGB_GRAY_MAP		= (int)28;
		public readonly int XA_RGB_GREEN_MAP		= (int)29;
		public readonly int XA_RGB_RED_MAP		= (int)30;
		public readonly int XA_STRING		= (int)31;
		public readonly int XA_VISUALID		= (int)32;
		public readonly int XA_WINDOW		= (int)33;
		public readonly int XA_WM_COMMAND		= (int)34;
		public readonly int XA_WM_HINTS		= (int)35;
		public readonly int XA_WM_CLIENT_MACHINE	= (int)36;
		public readonly int XA_WM_ICON_NAME		= (int)37;
		public readonly int XA_WM_ICON_SIZE		= (int)38;
		public readonly int XA_WM_NAME		= (int)39;
		public readonly int XA_WM_NORMAL_HINTS	= (int)40;
		public readonly int XA_WM_SIZE_HINTS		= (int)41;
		public readonly int XA_WM_ZOOM_HINTS		= (int)42;
		public readonly int XA_MIN_SPACE		= (int)43;
		public readonly int XA_NORM_SPACE		= (int)44;
		public readonly int XA_MAX_SPACE		= (int)45;
		public readonly int XA_END_SPACE		= (int)46;
		public readonly int XA_SUPERSCRIPT_X		= (int)47;
		public readonly int XA_SUPERSCRIPT_Y		= (int)48;
		public readonly int XA_SUBSCRIPT_X		= (int)49;
		public readonly int XA_SUBSCRIPT_Y		= (int)50;
		public readonly int XA_UNDERLINE_POSITION	= (int)51;
		public readonly int XA_UNDERLINE_THICKNESS	= (int)52;
		public readonly int XA_STRIKEOUT_ASCENT	= (int)53;
		public readonly int XA_STRIKEOUT_DESCENT	= (int)54;
		public readonly int XA_ITALIC_ANGLE		= (int)55;
		public readonly int XA_X_HEIGHT		= (int)56;
		public readonly int XA_QUAD_WIDTH		= (int)57;
		public readonly int XA_WEIGHT		= (int)58;
		public readonly int XA_POINT_SIZE		= (int)59;
		public readonly int XA_RESOLUTION		= (int)60;
		public readonly int XA_COPYRIGHT		= (int)61;
		public readonly int XA_NOTICE		= (int)62;
		public readonly int XA_FONT_NAME		= (int)63;
		public readonly int XA_FAMILY_NAME		= (int)64;
		public readonly int XA_FULL_NAME		= (int)65;
		public readonly int XA_CAP_HEIGHT		= (int)66;
		public readonly int XA_WM_CLASS		= (int)67;
		public readonly int XA_WM_TRANSIENT_FOR	= (int)68;

		public readonly int WM_PROTOCOLS;
		public readonly int WM_DELETE_WINDOW;
		public readonly int WM_TAKE_FOCUS;
		public readonly int _NET_SUPPORTED;
		public readonly int _NET_CLIENT_LIST;
		public readonly int _NET_NUMBER_OF_DESKTOPS;
		public readonly int _NET_DESKTOP_GEOMETRY;
		public readonly int _NET_DESKTOP_VIEWPORT;
		public readonly int _NET_CURRENT_DESKTOP;
		public readonly int _NET_DESKTOP_NAMES;
		public readonly int _NET_ACTIVE_WINDOW;
		public readonly int _NET_WORKAREA;
		public readonly int _NET_SUPPORTING_WM_CHECK;
		public readonly int _NET_VIRTUAL_ROOTS;
		public readonly int _NET_DESKTOP_LAYOUT;
		public readonly int _NET_SHOWING_DESKTOP;
		public readonly int _NET_CLOSE_WINDOW;
		public readonly int _NET_MOVERESIZE_WINDOW;
		public readonly int _NET_WM_MOVERESIZE;
		public readonly int _NET_RESTACK_WINDOW;
		public readonly int _NET_REQUEST_FRAME_EXTENTS;
		public readonly int _NET_WM_NAME;
		public readonly int _NET_WM_VISIBLE_NAME;
		public readonly int _NET_WM_ICON_NAME;
		public readonly int _NET_WM_VISIBLE_ICON_NAME;
		public readonly int _NET_WM_DESKTOP;
		public readonly int _NET_WM_WINDOW_TYPE;
		public readonly int _NET_WM_STATE;
		public readonly int _NET_WM_ALLOWED_ACTIONS;
		public readonly int _NET_WM_STRUT;
		public readonly int _NET_WM_STRUT_PARTIAL;
		public readonly int _NET_WM_ICON_GEOMETRY;
		public readonly int _NET_WM_ICON;
		public readonly int _NET_WM_PID;
		public readonly int _NET_WM_HANDLED_ICONS;
		public readonly int _NET_WM_USER_TIME;
		public readonly int _NET_FRAME_EXTENTS;
		public readonly int _NET_WM_PING;
		public readonly int _NET_WM_SYNC_REQUEST;
		public readonly int _NET_SYSTEM_TRAY_ORIENTATION;
		public readonly int _NET_SYSTEM_TRAY_OPCODE;
		public readonly int _NET_WM_STATE_MAXIMIZED_HORZ;
		public readonly int _NET_WM_STATE_MAXIMIZED_VERT;
		public readonly int _XEMBED;
		public readonly int _XEMBED_INFO;
		public readonly int _MOTIF_WM_HINTS;
		public readonly int _NET_WM_STATE_SKIP_TASKBAR;
		public readonly int _NET_WM_STATE_ABOVE;
		public readonly int _NET_WM_STATE_MODAL;
		public readonly int _NET_WM_STATE_HIDDEN;
		public readonly int _NET_WM_CONTEXT_HELP;
		public readonly int _NET_WM_WINDOW_OPACITY;
		public readonly int _NET_WM_WINDOW_TYPE_DESKTOP;
		public readonly int _NET_WM_WINDOW_TYPE_DOCK;
		public readonly int _NET_WM_WINDOW_TYPE_TOOLBAR;
		public readonly int _NET_WM_WINDOW_TYPE_MENU;
		public readonly int _NET_WM_WINDOW_TYPE_UTILITY;
		public readonly int _NET_WM_WINDOW_TYPE_SPLASH;
		public readonly int _NET_WM_WINDOW_TYPE_DIALOG;
		public readonly int _NET_WM_WINDOW_TYPE_NORMAL;
		public readonly int CLIPBOARD;
		public readonly int PRIMARY;
		public readonly int DIB;
		public readonly int OEMTEXT;
		public readonly int UNICODETEXT;
		public readonly int TARGETS;
		public readonly int PostAtom;
		public readonly int AsyncAtom;


		public X11Atoms (IntPtr display) {

			// make sure this array stays in sync with the statements below
			string [] atom_names = new string[] {
				"WM_PROTOCOLS",
				"WM_DELETE_WINDOW",
				"WM_TAKE_FOCUS",
				"_NET_SUPPORTED",
				"_NET_CLIENT_LIST",
				"_NET_NUMBER_OF_DESKTOPS",
				"_NET_DESKTOP_GEOMETRY",
				"_NET_DESKTOP_VIEWPORT",
				"_NET_CURRENT_DESKTOP",
				"_NET_DESKTOP_NAMES",
				"_NET_ACTIVE_WINDOW",
				"_NET_WORKAREA",
				"_NET_SUPPORTING_WM_CHECK",
				"_NET_VIRTUAL_ROOTS",
				"_NET_DESKTOP_LAYOUT",
				"_NET_SHOWING_DESKTOP",
				"_NET_CLOSE_WINDOW",
				"_NET_MOVERESIZE_WINDOW",
				"_NET_WM_MOVERESIZE",
				"_NET_RESTACK_WINDOW",
				"_NET_REQUEST_FRAME_EXTENTS",
				"_NET_WM_NAME",
				"_NET_WM_VISIBLE_NAME",
				"_NET_WM_ICON_NAME",
				"_NET_WM_VISIBLE_ICON_NAME",
				"_NET_WM_DESKTOP",
				"_NET_WM_WINDOW_TYPE",
				"_NET_WM_STATE",
				"_NET_WM_ALLOWED_ACTIONS",
				"_NET_WM_STRUT",
				"_NET_WM_STRUT_PARTIAL",
				"_NET_WM_ICON_GEOMETRY",
				"_NET_WM_ICON",
				"_NET_WM_PID",
				"_NET_WM_HANDLED_ICONS",
				"_NET_WM_USER_TIME",
				"_NET_FRAME_EXTENTS",
				"_NET_WM_PING",
				"_NET_WM_SYNC_REQUEST",
				"_NET_SYSTEM_TRAY_OPCODE",
				"_NET_SYSTEM_TRAY_ORIENTATION",
				"_NET_WM_STATE_MAXIMIZED_HORZ",
				"_NET_WM_STATE_MAXIMIZED_VERT",
				"_NET_WM_STATE_HIDDEN",
				"_XEMBED",
				"_XEMBED_INFO",
				"_MOTIF_WM_HINTS",
				"_NET_WM_STATE_SKIP_TASKBAR",
				"_NET_WM_STATE_ABOVE",
				"_NET_WM_STATE_MODAL",
				"_NET_WM_CONTEXT_HELP",
				"_NET_WM_WINDOW_OPACITY",
				"_NET_WM_WINDOW_TYPE_DESKTOP",
				"_NET_WM_WINDOW_TYPE_DOCK",
				"_NET_WM_WINDOW_TYPE_TOOLBAR",
				"_NET_WM_WINDOW_TYPE_MENU",
				"_NET_WM_WINDOW_TYPE_UTILITY",
				"_NET_WM_WINDOW_TYPE_DIALOG",
				"_NET_WM_WINDOW_TYPE_SPLASH",
				"_NET_WM_WINDOW_TYPE_NORMAL",
				"CLIPBOARD",
				"PRIMARY",
				"COMPOUND_TEXT",
				"UTF8_STRING",
				"TARGETS",
				"_SWF_AsyncAtom",
				"_SWF_PostMessageAtom",
				"_SWF_HoverAtom" };

			int[] atoms = new int [atom_names.Length];;

			Xlib.XInternAtoms (display, atom_names, atom_names.Length, false, atoms);

			int off = 0;
			WM_PROTOCOLS = atoms [off++];
			WM_DELETE_WINDOW = atoms [off++];
			WM_TAKE_FOCUS = atoms [off++];
			_NET_SUPPORTED = atoms [off++];
			_NET_CLIENT_LIST = atoms [off++];
			_NET_NUMBER_OF_DESKTOPS = atoms [off++];
			_NET_DESKTOP_GEOMETRY = atoms [off++];
			_NET_DESKTOP_VIEWPORT = atoms [off++];
			_NET_CURRENT_DESKTOP = atoms [off++];
			_NET_DESKTOP_NAMES = atoms [off++];
			_NET_ACTIVE_WINDOW = atoms [off++];
			_NET_WORKAREA = atoms [off++];
			_NET_SUPPORTING_WM_CHECK = atoms [off++];
			_NET_VIRTUAL_ROOTS = atoms [off++];
			_NET_DESKTOP_LAYOUT = atoms [off++];
			_NET_SHOWING_DESKTOP = atoms [off++];
			_NET_CLOSE_WINDOW = atoms [off++];
			_NET_MOVERESIZE_WINDOW = atoms [off++];
			_NET_WM_MOVERESIZE = atoms [off++];
			_NET_RESTACK_WINDOW = atoms [off++];
			_NET_REQUEST_FRAME_EXTENTS = atoms [off++];
			_NET_WM_NAME = atoms [off++];
			_NET_WM_VISIBLE_NAME = atoms [off++];
			_NET_WM_ICON_NAME = atoms [off++];
			_NET_WM_VISIBLE_ICON_NAME = atoms [off++];
			_NET_WM_DESKTOP = atoms [off++];
			_NET_WM_WINDOW_TYPE = atoms [off++];
			_NET_WM_STATE = atoms [off++];
			_NET_WM_ALLOWED_ACTIONS = atoms [off++];
			_NET_WM_STRUT = atoms [off++];
			_NET_WM_STRUT_PARTIAL = atoms [off++];
			_NET_WM_ICON_GEOMETRY = atoms [off++];
			_NET_WM_ICON = atoms [off++];
			_NET_WM_PID = atoms [off++];
			_NET_WM_HANDLED_ICONS = atoms [off++];
			_NET_WM_USER_TIME = atoms [off++];
			_NET_FRAME_EXTENTS = atoms [off++];
			_NET_WM_PING = atoms [off++];
			_NET_WM_SYNC_REQUEST = atoms [off++];
			_NET_SYSTEM_TRAY_OPCODE = atoms [off++];
			_NET_SYSTEM_TRAY_ORIENTATION = atoms [off++];
			_NET_WM_STATE_MAXIMIZED_HORZ = atoms [off++];
			_NET_WM_STATE_MAXIMIZED_VERT = atoms [off++];
			_NET_WM_STATE_HIDDEN = atoms [off++];
			_XEMBED = atoms [off++];
			_XEMBED_INFO = atoms [off++];
			_MOTIF_WM_HINTS = atoms [off++];
			_NET_WM_STATE_SKIP_TASKBAR = atoms [off++];
			_NET_WM_STATE_ABOVE = atoms [off++];
			_NET_WM_STATE_MODAL = atoms [off++];
			_NET_WM_CONTEXT_HELP = atoms [off++];
			_NET_WM_WINDOW_OPACITY = atoms [off++];
			_NET_WM_WINDOW_TYPE_DESKTOP = atoms [off++];
			_NET_WM_WINDOW_TYPE_DOCK = atoms [off++];
			_NET_WM_WINDOW_TYPE_TOOLBAR = atoms [off++];
			_NET_WM_WINDOW_TYPE_MENU = atoms [off++];
			_NET_WM_WINDOW_TYPE_UTILITY = atoms [off++];
			_NET_WM_WINDOW_TYPE_DIALOG = atoms [off++];
			_NET_WM_WINDOW_TYPE_SPLASH = atoms [off++];
			_NET_WM_WINDOW_TYPE_NORMAL = atoms [off++];
			CLIPBOARD = atoms [off++];
			PRIMARY = atoms [off++];
			OEMTEXT = atoms [off++];
			UNICODETEXT = atoms [off++];
			TARGETS = atoms [off++];
			AsyncAtom = atoms [off++];
			PostAtom = atoms [off++];

			DIB = XA_PIXMAP;
		}

	}

}

