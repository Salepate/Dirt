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

        public virtual string ContentPath => Path.Combine(Application.dataPath, "Content");
        public virtual string ContentName => "gamecontent";

        public Action OnContentReloadAction { get; set; }

        private ContentProvider m_ContentProvider;
        public override void Initialize(DirtMode mode)
        {
            m_ContentProvider = new ContentProvider(ContentPath);
            m_ContentProvider.LoadGameContent(ContentName);
        }
    }
}