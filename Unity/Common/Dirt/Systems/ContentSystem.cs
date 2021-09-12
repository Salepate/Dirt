using Dirt.Game;
using Dirt.Game.Content;
using System;
using System.IO;
using UnityEngine;

namespace Dirt.Systems
{
    [System.Serializable]
    public class ContentSystem : DirtSystem
    {
        public IContentProvider Content { get { return m_ContentProvider; } }

        public Action OnContentReloadAction { get; set; }

        private ContentProvider m_ContentProvider;
        public override void Initialize(DirtMode mode)
        {
            string contentPath = Application.dataPath;
#if UNITY_EDITOR
            contentPath = Path.Combine(Application.dataPath, @"..\..\Content");
#else
            contentPath = Path.Combine(contentPath,"Content");
#endif
            m_ContentProvider = new ContentProvider(contentPath);
            m_ContentProvider.LoadGameContent("gamecontent");

        }
    }
}