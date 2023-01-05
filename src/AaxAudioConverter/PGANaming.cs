﻿using audiamus.aaxconv.lib;
using audiamus.aux.ex;
using audiamus.aux.propgrid;
using System;
using System.ComponentModel;
using System.Media;
using System.Resources;

namespace audiamus.aaxconv
{
    class PGANaming : BasePropertyGridAdapter<INamingSettings>, INamingSettings
    {

        #region const
        const string AUDIOBOOK = "Audiobook";
        const string CHAPTER = "Chapter";
        const string PART = "Part";
        #endregion const

        #region Public Properties

        [Browsable(false)]
        public Func<bool> IsInSplitChapterMode { private get; set; }

        [PropertyOrder(1)]
        [TypeConverter(typeof(EnumChainConverterRM<EFileNaming, ChainPunctuationDash>))]
        public EFileNaming FileNaming
        {
            get => DataSource.FileNaming;
            set => DataSource.FileNaming = value;
        }

        [PropertyOrder(2)]
        [TypeConverter(typeof(EnumChainConverterRM<ETitleNaming, ChainPunctuationDash>))]
        public ETitleNaming TitleNaming
        {
            get => DataSource.TitleNaming;
            set => DataSource.TitleNaming = value;
        }

        [PropertyOrder(3)]
        [TypeConverter(typeof(EnumChainConverterRM<ETrackNumbering, ChainPunctuationDot>))]
        public ETrackNumbering TrackNumbering
        {
            get => DataSource.TrackNumbering;
            set
            {
                if (ChapterNaming == EGeneralNamingEx._nofolders && value == ETrackNumbering.chapter_a_track)
                {
                    SystemSounds.Hand.Play();
                    return;
                }

                DataSource.TrackNumbering = value;
            }
        }

        [PropertyOrder(4)]
        [TypeConverter(typeof(BooleanYesNoConverterRM))]
        public bool TotalTracks
        {
            get => DataSource.TotalTracks;
            set => DataSource.TotalTracks = value;
        }

        [PropertyOrder(5)]
        [TypeConverter(typeof(EnumChainConverterRM<EGeneralNaming, ChainPunctuationBracket>))]
        public EGeneralNaming GenreNaming
        {
            get => DataSource.GenreNaming;
            set
            {
                DataSource.GenreNaming = value;
                Update();
            }
        }

        [PropertyOrder(6)]
        public string GenreName
        {
            get => DataSource.GenreName;
            set => DataSource.GenreName = value;
        }

        [PropertyOrder(7)]
        [TypeConverter(typeof(EnumChainConverterRM<EGeneralNamingEx, ChainPunctuationBracket>))]
        public EGeneralNamingEx ChapterNaming
        {
            get => DataSource.ChapterNaming;
            set
            {
                if (value == EGeneralNamingEx._nofolders && TrackNumbering == ETrackNumbering.chapter_a_track)
                {
                    SystemSounds.Hand.Play();
                    return;
                }
                DataSource.ChapterNaming = value;
                Update();
            }
        }

        [PropertyOrder(8)]
        public string ChapterName
        {
            get => DataSource.ChapterName;
            set => DataSource.ChapterName = value;
        }

        [PropertyOrder(9)]
        [TypeConverter(typeof(BooleanYesNoConverterRM))]
        public bool SeriesTitleLeft
        {
            get => DataSource.SeriesTitleLeft;
            set => DataSource.SeriesTitleLeft = value;
        }

        [PropertyOrder(10)]
        [TypeConverter(typeof(EnumConverterRM<ELongTitle>))]
        public ELongTitle LongBookTitle
        {
            get => DataSource.LongBookTitle;
            set => DataSource.LongBookTitle = value;
        }

        #region Private Properties
        private ResourceManager ResourceManager { get; set; }
        #endregion Private Properties

        #endregion Public Properties
        #region Public Constructors

        public PGANaming(IAppSettings datasource) : base(datasource)
        {
            ResourceManager = this.GetDefaultResourceManager();
            Update();
        }

        #endregion Public Constructors

        #region Public Methods

        public void Update()
        {
            PropertyCommands[nameof(ChapterName)].ReadOnly = ChapterNaming != EGeneralNamingEx.custom;
            PropertyCommands[nameof(GenreName)].ReadOnly = GenreNaming != EGeneralNaming.custom;
            PropertyCommands[nameof(TrackNumbering)].ReadOnly = !(IsInSplitChapterMode?.Invoke() ?? true);
            PropertyCommands[nameof(ChapterNaming)].ReadOnly = !(IsInSplitChapterMode?.Invoke() ?? true);
            naming();
            RefreshDelegate?.Invoke();
        }

        #endregion Public Methods
        #region Private Methods

        private void naming()
        {
            if (DataSource.GenreNaming == EGeneralNaming.standard)
                DataSource.GenreName = ResourceManager.GetStringEx(AUDIOBOOK);
            if (DataSource.ChapterNaming == EGeneralNamingEx.standard)
                DataSource.ChapterName = ResourceManager.GetStringEx(CHAPTER);
        }

        #endregion Private Methods
    }
}
