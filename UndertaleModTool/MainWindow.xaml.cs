﻿using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using UndertaleModLib;
using UndertaleModLib.Models;

namespace UndertaleModTool
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public UndertaleData Data { get; set; }
        public string FilePath { get; set; }

        private object _Highlighted;
        public object Highlighted { get { return _Highlighted; } set { _Highlighted = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Highlighted")); } }
        private object _Selected;
        public object Selected { get { return _Selected; } set { _Selected = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Selected")); } }

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            Selected = Highlighted = new DescriptionView("Welcome to UndertaleModTool!", "Open data.win file to get started, then double click on the items on the left to view them");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Command_Open(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.DefaultExt = "win";
            dlg.Filter = "Game Maker Studio data files (.win, .unx, .ios)|*.win;*.unx;*.ios|All files|*";
            
            if (dlg.ShowDialog() == true)
            {
                LoadFile(dlg.FileName);
            }
        }

        private void Command_Save(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            
            dlg.DefaultExt = "win";
            dlg.Filter = "Game Maker Studio data files (.win, .unx, .ios)|*.win;*.unx;*.ios|All files|*";
            dlg.FileName = FilePath;

            if (dlg.ShowDialog() == true)
            {
                SaveFile(dlg.FileName);
            }
        }

        private void Command_Close(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private async void LoadFile(string filename)
        {
            LoaderDialog dialog = new LoaderDialog("Loading", "Loading, please wait...");
            dialog.Owner = this;
            Task t = Task.Run(() =>
            {
                UndertaleData data = null;
                try
                {
                    using (var stream = new FileStream(filename, FileMode.Open))
                    {
                        data = UndertaleIO.Read(stream);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("An error occured while trying to load:\n" + e.Message, "Load error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                Dispatcher.Invoke(() =>
                {
                    if (data != null)
                    {
                        this.Data = data;
                        this.FilePath = filename;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Data"));
                        Selected = Highlighted = new DescriptionView("Welcome to UndertaleModTool!", "Double click on the items on the left to view them!");
                    }
                    dialog.Hide();
                });
            });
            dialog.ShowDialog();
            await t;
        }

        private async void SaveFile(string filename)
        {
            if (Data == null)
                return; // TODO: lock save button
            LoaderDialog dialog = new LoaderDialog("Saving", "Saving, please wait...");
            dialog.Owner = this;
            Task t = Task.Run(() =>
            {
                try
                {
                    using (var stream = new FileStream(filename, FileMode.Create))
                    {
                        UndertaleIO.Write(stream, Data);
                    }
                }
                catch(Exception e)
                {
                    MessageBox.Show("An error occured while trying to save:\n" + e.Message, "Save error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                Dispatcher.Invoke(() =>
                {
                    dialog.Hide();
                });
            });
            dialog.ShowDialog();
            await t;
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is TreeViewItem)
            {
                string item = (e.NewValue as TreeViewItem)?.Header?.ToString();
                if (item == "General info" && Data != null)
                {
                    Highlighted = new GeneralInfoEditor(Data?.GeneralInfo, Data?.Options, Data?.Language);
                }
                else if (item == "Data")
                {
                    Highlighted = new DescriptionView("Welcome to UndertaleModTool!", Data != null ? "Double click on the items on the left to view them" : "Open data.win file to get started");
                }
                else if (item == "Code locals (unused?)")
                {
                    Highlighted = new DescriptionView(item, Data != null ? "This seems to be unused as far as I can tell - you can remove the whole list and nothing happens" : "Load data.win file first");
                }
                else if (item == "Variables")
                {
                    Highlighted = Data != null ? (object)Data.FORM.Chunks["VARI"] : new DescriptionView(item, "Load data.win file first");
                }
                else
                {
                    Highlighted = new DescriptionView(item, Data != null ? "Expand the list on the left to edit items" : "Load data.win file first");
                }
            }
            else
            {
                Highlighted = e.NewValue;
            }
        }

        private void MainTree_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Selected = Highlighted;
        }

        private void TreeView_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragDropEffects effects = DragDropEffects.Move | DragDropEffects.Link;

                UndertaleObject draggedItem = Highlighted as UndertaleObject;
                if (draggedItem != null)
                {
                    DataObject data = new DataObject(draggedItem);
                    //data.SetText(draggedItem.ToString());
                    /*if (draggedItem is UndertaleEmbeddedTexture)
                    {
                        UndertaleEmbeddedTexture tex = draggedItem as UndertaleEmbeddedTexture;
                        MemoryStream ms = new MemoryStream(tex.TextureData.TextureBlob);
                        PngBitmapDecoder decoder = new PngBitmapDecoder(ms, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                        data.SetImage(decoder.Frames[0]);
                        Debug.WriteLine("PNG data attached");
                        effects |= DragDropEffects.Copy;
                    }*/

                    DragDrop.DoDragDrop(MainTree, data, effects);
                }
            }
        }

        private void TreeView_DragOver(object sender, DragEventArgs e)
        {
            UndertaleObject sourceItem = e.Data.GetData(e.Data.GetFormats()[e.Data.GetFormats().Length-1]) as UndertaleObject; // TODO: make this more reliable

            foreach (var s in e.Data.GetFormats())
                Debug.WriteLine(s);

            TreeViewItem targetTreeItem = VisualUpwardSearch<TreeViewItem>(e.OriginalSource as UIElement);
            UndertaleObject targetItem = targetTreeItem.DataContext as UndertaleObject;

            e.Effects = e.AllowedEffects.HasFlag(DragDropEffects.Move) && sourceItem != null && targetItem != null && sourceItem != targetItem && sourceItem.GetType() == targetItem.GetType() ? DragDropEffects.Move : DragDropEffects.None;
            e.Handled = true;
        }

        private void TreeView_Drop(object sender, DragEventArgs e)
        {
            UndertaleObject sourceItem = e.Data.GetData(e.Data.GetFormats()[e.Data.GetFormats().Length - 1]) as UndertaleObject;

            TreeViewItem targetTreeItem = VisualUpwardSearch<TreeViewItem>(e.OriginalSource as UIElement);
            UndertaleObject targetItem = targetTreeItem.DataContext as UndertaleObject;

            e.Effects = e.AllowedEffects.HasFlag(DragDropEffects.Move) && sourceItem != null && targetItem != null && sourceItem != targetItem && sourceItem.GetType() == targetItem.GetType() ? DragDropEffects.Move : DragDropEffects.None;
            if (e.Effects == DragDropEffects.Move)
            {
                IList list = GetNearestParent<TreeViewItem>(targetTreeItem).ItemsSource as IList;
                int sourceIndex = list.IndexOf(sourceItem);
                int targetIndex = list.IndexOf(targetItem);
                Debug.Assert(sourceIndex >= 0 && targetIndex >= 0);
                list[sourceIndex] = targetItem;
                list[targetIndex] = sourceItem;
            }
            e.Handled = true;
        }
        
        private static T VisualUpwardSearch<T>(DependencyObject element) where T : class
        {
            T container = element as T;
            while (container == null && element != null)
            {
                element = VisualTreeHelper.GetParent(element) as DependencyObject;
                container = element as T;
            }
            return container;
        }

        private static T GetNearestParent<T>(DependencyObject item) where T : class
        {
            DependencyObject parent = VisualTreeHelper.GetParent(item);
            while (!(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent as T;
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        public static childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
        {
            foreach (childItem child in FindVisualChildren<childItem>(obj))
            {
                return child;
            }

            return null;
        }

        private TreeViewItem GetTreeViewItemFor(UndertaleObject obj)
        {
            foreach (var child in (MainTree.Items[0] as TreeViewItem).Items)
            {
                var twi = (child as TreeViewItem).ItemContainerGenerator.ContainerFromItem(obj) as TreeViewItem;
                if (twi != null)
                    return twi;
            }
            return null;
        }

        private void DeleteItem(UndertaleObject obj)
        {
            TreeViewItem container = GetNearestParent<TreeViewItem>(GetTreeViewItemFor(obj));
            IList list = container.ItemsSource as IList;
            bool isLast = list.IndexOf(obj) == list.Count-1;
            if (MessageBox.Show("Delete " + obj.ToString() + "?" + (!isLast ? "\n\nNote that the code often references objects by ID, so this operation is likely to break stuff because other items will shift up!" : ""), "Confirmation", MessageBoxButton.YesNo, isLast ? MessageBoxImage.Question : MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                list.Remove(obj);
                Selected = null;
                if (Highlighted == obj)
                    Highlighted = null;
            }
        }

        private void MainTree_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                if (Selected != null && Selected is UndertaleObject)
                {
                    UndertaleObject obj = Selected as UndertaleObject;
                    DeleteItem(obj);
                }
            }
        }
        
        private async void CommandBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                Debug.WriteLine(CommandBox.Text);
                e.Handled = true;
                CommandBox.IsEnabled = false;
                object result;
                try
                {
                    result = await CSharpScript.EvaluateAsync(CommandBox.Text, ScriptOptions.Default
                        .WithReferences(typeof(UndertaleObject).Assembly)
                        .WithImports("UndertaleModLib", "UndertaleModLib.Models", "System", "System.IO", "System.Collections.Generic"), this);
                }
                catch (CompilationErrorException exc)
                {
                    result = exc.Message;
                    Debug.WriteLine(exc);
                }
                catch (Exception exc)
                {
                    result = exc;
                }
                CommandBox.Text = result != null ? result.ToString() : "";
                CommandBox.IsEnabled = true;
            }
        }

        private void Command_Copy(object sender, ExecutedRoutedEventArgs e)
        {
            // TODO: ugly, but I can't get focus to work properly
            /*var command = FindVisualChild<UndertaleRoomEditor>(DataEditor)?.CommandBindings.OfType<CommandBinding>()
                .FirstOrDefault(cmd => cmd.Command == e.Command);

            if (command != null && command.Command.CanExecute(e.Parameter))
                command.Command.Execute(e.Parameter);*/
            FindVisualChild<UndertaleRoomEditor>(DataEditor)?.Command_Copy(sender, e);
        }

        private void Command_Paste(object sender, ExecutedRoutedEventArgs e)
        {
            FindVisualChild<UndertaleRoomEditor>(DataEditor)?.Command_Paste(sender, e);
        }

        private void MainTree_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject);

            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                e.Handled = true;
            }
        }

        private void MenuItem_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (Highlighted != null && Highlighted is UndertaleObject)
                DeleteItem(Highlighted as UndertaleObject);
        }

        private void MenuItem_Add_Click(object sender, RoutedEventArgs e)
        {
            IList list = (MainTree.SelectedItem as TreeViewItem).ItemsSource as IList;
            Type t = list.GetType().GetGenericArguments()[0];
            Debug.Assert(typeof(UndertaleResource).IsAssignableFrom(t));
            UndertaleResource obj = Activator.CreateInstance(t) as UndertaleResource;
            if (obj is UndertaleNamedResource)
            {
                string newname = obj.GetType().Name.Replace("Undertale", "").Replace("GameObject", "Object").ToLower() + list.Count;
                (obj as UndertaleNamedResource).Name = Data.Strings.MakeString(newname);
            }
            list.Add(obj);
            // TODO: change highlighted
            Selected = obj;
        }
    }

    public class GeneralInfoEditor
    {
        public UndertaleGeneralInfo GeneralInfo { get; private set; }
        public UndertaleOptions Options { get; private set; }
        public UndertaleLanguage Language { get; private set; }

        public GeneralInfoEditor(UndertaleGeneralInfo generalInfo, UndertaleOptions options, UndertaleLanguage language)
        {
            this.GeneralInfo = generalInfo;
            this.Options = options;
            this.Language = language;
        }
    }

    public class DescriptionView
    {
        public string Heading { get; private set; }
        public string Description { get; private set; }

        public DescriptionView(string heading, string description)
        {
            Heading = heading;
            Description = description;
        }
    }
}
