﻿using Microsoft.Win32;
using SqliteEditor.Utilities;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace SqliteEditor.ViewModels
{
    public class PathViewModel : CompositeDisposableBase
    {
        public PathViewModel(string path)
            : this()
        {
            Path.Value = path;
        }

        public PathViewModel()
        {
            Subscribe(RootPath, value => UpdatePathAndActualPath());
            Subscribe(Path, value => UpdatePathAndActualPath());
            Subscribe(OpenPathCommand, () =>
            {
                var dialog = new OpenFileDialog();
                if (!IsEmpty())
                {
                    dialog.InitialDirectory = System.IO.Path.GetDirectoryName(ActualPath.Value);
                }
                dialog.Filter = "全てのファイル (*.*)|*.*";
                if (dialog.ShowDialog() == true)
                {
                    Path.Value = dialog.FileName;
                }
            });
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(Path.Value);
        }

        public bool IsValidPath()
        {
            return !IsEmpty() && (File.Exists(Path.Value) || Directory.Exists(Path.Value));
        }

        public ReactiveProperty<string> Path { get; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> RootPath { get; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> ActualPath { get; } = new ReactiveProperty<string>();

        public ReactiveCommand OpenPathCommand { get; } = new ReactiveCommand();

        private void UpdatePathAndActualPath()
        {
            UpdatePath();
            UpdateActualPath();
        }

        private void UpdatePath()
        {
            if (System.IO.Path.IsPathRooted(Path.Value))
            {
                var dirName = System.IO.Path.GetDirectoryName(Path.Value);
                if (dirName is not null && PathUtility.ComparePaths(dirName, RootPath.Value))
                {
                    Path.Value = System.IO.Path.GetFileName(Path.Value);
                }
            }
        }

        public void UpdateActualPath()
        {
            if (string.IsNullOrEmpty(Path.Value) || string.IsNullOrEmpty(RootPath.Value) || System.IO.Path.IsPathRooted(Path.Value))
            {
                ActualPath.Value = Path.Value;
            }
            else
            {
                ActualPath.Value = System.IO.Path.Combine(RootPath.Value, Path.Value);
            }
        }
    }
}
