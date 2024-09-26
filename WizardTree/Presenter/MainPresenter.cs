using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WizardTree.Models;
using WizardTree.Views;

namespace WizardTree.Presenter
{
    public class MainPresenter
    {
        private readonly MainWindow _view;
        private FileSystemItem _rootItem;

        public MainPresenter(MainWindow view)
        {
            _view = view;
        }

        public async Task InitializeFileSystemAsync()
        {
            //_view.ProgressBar.IsIndeterminate = true;
            _rootItem = new FileSystemItem("Этот компьютер", isComputerNode: true);
            await _rootItem.LoadChildrenAsync();
            _view.FileSystemTreeView.ItemsSource = new List<FileSystemItem> { _rootItem };
            //_view.ProgressBar.IsIndeterminate = false;
        }

        public void SortItems(SortOption sortOption)
        {
            SortChildren(_rootItem, sortOption);
            _view.FileSystemTreeView.Items.Refresh();
        }

        private void SortChildren(FileSystemItem item, SortOption sortOption)
        {
            if (item.Children.Count > 0)
            {
                List<FileSystemItem> sortedChildren;
                switch (sortOption)
                {
                    case SortOption.SizeAscending:
                        sortedChildren = item.Children.OrderBy(c => c.Size).ToList();
                        break;
                    case SortOption.SizeDescending:
                        sortedChildren = item.Children.OrderByDescending(c => c.Size).ToList();
                        break;
                    case SortOption.NameAscending:
                        sortedChildren = item.Children.OrderBy(c => c.Name).ToList();
                        break;
                    case SortOption.NameDescending:
                        sortedChildren = item.Children.OrderByDescending(c => c.Name).ToList();
                        break;
                    default:
                        return;
                }

                item.Children.Clear();
                foreach (var child in sortedChildren)
                {
                    item.Children.Add(child);
                    SortChildren(child, sortOption);
                }
            }
        }
    }

    public enum SortOption
    {
        SizeAscending,
        SizeDescending,
        NameAscending,
        NameDescending
    }
}
