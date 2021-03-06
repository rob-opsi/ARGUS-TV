/*
 *	Copyright (C) 2007-2014 ARGUS TV
 *	http://www.argus-tv.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;

namespace ArgusTV.UI.Console.Controls
{
    public class MenuHeaderLabel : Label
    {
        private PictureBox _pictureBox;

        public MenuHeaderLabel()
        {
            _pictureBox = new PictureBox();
            _pictureBox.BackgroundImage = Properties.Resources.MenuHeaderBullet;
            _pictureBox.BackgroundImageLayout = ImageLayout.Center;
            _pictureBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;

            this.Padding = new Padding(8, 0, 0, 0);
            this.TextAlign = ContentAlignment.MiddleLeft;
        }

        protected override Padding DefaultPadding
        {
            get { return new Padding(8, 0, 0, 0); }
        }

        [DefaultValue(ContentAlignment.MiddleLeft)]
        public override ContentAlignment TextAlign
        {
            get { return base.TextAlign; }
            set { base.TextAlign = value; }
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            _pictureBox.Location = new Point(0, 2);
            _pictureBox.Size = new Size(6, this.Height - 2);
            this.Controls.Add(_pictureBox);
        }
    }
}
