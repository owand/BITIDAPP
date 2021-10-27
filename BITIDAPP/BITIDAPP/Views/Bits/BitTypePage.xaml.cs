using BITIDAPP.Models.Bits;
using BITIDAPP.Resources;
using System;
using System.IO;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BITIDAPP.Views.Bits
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BitTypePage : ContentPage
    {
        private BitType viewModel;
        public BitTypeJoin NewItem = null; // Новая запись

        public BitTypePage()
        {
            InitializeComponent();
            LayoutChanged += OnSizeChanged; // Определяем обработчик события, которое происходит, когда изменяется ширина или высота.
            Shell.Current.Navigating += Current_Navigating; // Определяем обработчик события Shell.OnNavigating
        }

        // События непосредственно перед тем как страница становится видимой.
        protected override void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                indicator.IsRunning = true;
                IsBusy = true; ;  // Затеняем задний фон и запускаем ProgressRing

                Xamarin.Forms.Device.BeginInvokeOnMainThread(async () =>
                {
                    if (viewModel == null) // Если не открыт Picer для выбора картинки в Android
                    {
                        viewModel = new BitType(string.Empty);
                    }

                    BindingContext = viewModel;

                    if (viewModel.SelectedJoinItem == null) // Если не открыт Picer для выбора картинки в Android
                    {
                        viewModel.SelectedJoinItem = viewModel?.JoinCollection?.FirstOrDefault(); // Переходим на первую запись.
                    }

                    viewModel.DetailMode = false;

                    await System.Threading.Tasks.Task.Delay(100);
                });

                IsBusy = false;
                indicator.IsRunning = false;
            }
            catch (Exception ex)
            {
                Device.BeginInvokeOnMainThread(async () => { await DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); }); // Что-то пошло не так
                return;
            }
        }

        private void Current_Navigating(object sender, ShellNavigatingEventArgs e)
        {
            if (e.CanCancel)
            {
                e.Cancel(); // Позволяет отменить навигацию
                OnBackButtonPressed();
            }
        }

        // Происходит, когда ширина или высота свойств измените значение на этот элемент.
        private void OnSizeChanged(object sender, EventArgs e)
        {
            OnSizeChangeInterface(); // Изменение интерфейса при изменении размера окна
        }

        // Событие, которое вызывается при выборе отличного от текущего или нового элемента.
        private void OnSelection(object sender, SelectedItemChangedEventArgs e) // Загружаем дочернюю форму и передаем ей параметр ID.
        {
            try
            {
                if (e.SelectedItem != null) // Если в Collection есть записи.
                {
                    EditButton.IsEnabled = true; // Кнопка Редактирования активна.
                    DeleteButton.IsEnabled = true; // Кнопка Удаления записи активна.
                }
                else
                {
                    EditButton.IsEnabled = false; // Кнопка Редактирования неактивна.
                    DeleteButton.IsEnabled = false; // Кнопка Удаления записи неактивна.
                }
            }
            catch (Exception ex)
            {
                // Что-то пошло не так
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk);
                });
                return;
            }
        }

        private void OnTapped(object sender, ItemTappedEventArgs e)
        {
            try
            {
                switch (Device.Idiom)
                {
                    case TargetIdiom.Desktop:
                    case TargetIdiom.Tablet:
                        if (Shell.Current.Width <= 800)
                        {
                            viewModel.DetailMode = true;
                            OnSizeChangeInterface(); // Изменение интерфейса при изменении размера окна
                        }
                        break;

                    case TargetIdiom.Phone:
                        viewModel.DetailMode = true;
                        OnSizeChangeInterface(); // Изменение интерфейса при изменении размера окна
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Device.BeginInvokeOnMainThread(async () => { await DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); }); // Что-то пошло не так
                return;
            }
        }

        // Фильтр записей отображаемых в ListView.
        private void OnFilter(object sender, TextChangedEventArgs e)
        {
            try
            {
                // Обновление записей в ListView Collection
                RefreshListView();
            }
            catch (Exception ex)
            {
                // Что-то пошло не так
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk);
                });
                return;
            }
        }

        private void DeleteImage(object sender, EventArgs e)
        {
            try
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    bool dialog = await DisplayAlert(AppResource.messageTitleAction, AppResource.messageDelete, AppResource.messageOk, AppResource.messageСancel);
                    if (dialog == true)
                    {
                        viewModel.SelectedJoinItem.PICTURE = null;
                    }
                    else
                    {
                        return;
                    }
                });
            }
            catch (Exception ex)
            {
                // Что-то пошло не так
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk);
                });
                return;
            }
        }

        private async void UploadImage(object sender, EventArgs args)
        {
            try
            {
                Plugin.FilePicker.Abstractions.FileData fileData = await Plugin.FilePicker.CrossFilePicker.Current.PickFile();
                if (fileData != null)
                {
                    MemoryStream memoryStream = new MemoryStream();
                    Stream stream = fileData.GetStream();
                    stream.CopyTo(memoryStream);
                    stream.Dispose();
                    viewModel.SelectedJoinItem.PICTURE = memoryStream.ToArray();
                }

                switch (Device.Idiom)
                {
                    //case TargetIdiom.Desktop:
                    //case TargetIdiom.Tablet:
                    //    if (Shell.Current.Width <= 800)
                    //    {
                    //        await BuildIt.Forms.VisualStateManager.GoToState(this, "DetailsState"); // Компактный вид, режим чтения, только детали (список мастера скрыт).
                    //    }
                    //    else
                    //    {
                    //        await BuildIt.Forms.VisualStateManager.GoToState(this, "DefaultState"); // Расширенный (полный) вид, режим чтения.
                    //    }

                    //    break;

                    case TargetIdiom.Phone:
                        GoToEditState(); // Переходим в режим чтения.
                        break;

                    default: break;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk);
            }
        }

        #region --------- Header - Command --------

        // Переходим в режим редактирования.
        private void OnEdit(object sender, EventArgs e)
        {
            try
            {
                NewItem = null;

                if (MasterContent.SelectedItem != null)
                {
                    GoToEditState(); // Переходим в режим редактирования.
                }
                else
                {
                    // Что-то пошло не так
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await DisplayAlert(AppResource.messageAttention, AppResource.messageNoActiveRecord, null, AppResource.messageСancel);
                    });
                    EditButton.IsEnabled = false; // Если нет активной записи в MasterListView, кнопка Редактирования неактивна.
                    return;
                }
            }
            catch (Exception ex)
            {
                // Что-то пошло не так
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk);
                });
                return;
            }
        }

        // Создаем новую запись.
        private void OnAdd(object sender, EventArgs e)
        {
            // Создаем новую запись в объединенной коллекции
            NewItem = viewModel?.NewItem;
            try
            {
                viewModel?.JoinCollection?.Add(viewModel?.NewItem);
                viewModel.SelectedJoinItem = viewModel?.NewItem;
                MasterContent.ScrollTo(viewModel.SelectedJoinItem, ScrollToPosition.Center, true); // Прокручиваем Scroll до активной записи.

                GoToEditState(); // Переходим в режим редактирования.
            }
            catch (Exception ex)
            {
                // Что-то пошло не так
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk);
                });
                return;
            }
        }

        // Удаляем текущую запись.async
        private void OnDelete(object sender, EventArgs e)
        {
            try
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    bool dialog = await DisplayAlert(AppResource.messageTitleAction, AppResource.messageDelete, AppResource.messageOk, AppResource.messageСancel);
                    if (dialog == true)
                    {
                        int indexItem = viewModel.JoinCollection.IndexOf(viewModel.SelectedJoinItem);
                        // Удаляем текущую запись.
                        viewModel?.DeleteItem();

                        if (viewModel?.JoinCollection?.FirstOrDefault() != null) // Если в Collection есть записи.
                        {
                            if (indexItem == 0) // Если текущая запись первая.
                            {
                                viewModel.SelectedJoinItem = viewModel?.JoinCollection?[indexItem]; // Переходим на следующую запись после удаленной, у которой такой же индекс как и у удаленной.
                                return;
                            }
                            else
                            {
                                viewModel.SelectedJoinItem = viewModel?.JoinCollection[indexItem - 1]; // Переходим на предыдующую запись перед удаленной.
                            }
                        }
                    }
                    else
                    {
                        return;
                    }
                });
            }
            catch (Exception ex)
            {
                // Что-то пошло не так
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk);
                });
                return;
            }
        }

        // Сохраняем изменения.
        private void OnSave(object sender, EventArgs e)
        {
            try
            {
                BitTypeModel selectItem = null;
                BitTypeModel newItem = null;
                BitTypeMLModel selectMLItem = null;
                BitTypeMLModel newMLItem = null;

                //if (viewModel.Collection.FirstOrDefault(c => c.TYPEID == viewModel.SelectedJoinItem.TYPEID) != null)
                if (NewItem == null)
                {
                    // Данные для основной коллекции
                    selectItem = viewModel.Collection.FirstOrDefault(c => c.TYPEID == viewModel.SelectedJoinItem.TYPEID);
                    selectItem.TYPENAME = editNAME.Text;
                    if (viewModel.SelectedJoinItem.PICTURE != null)
                    {
                        selectItem.PICTURE = viewModel.SelectedJoinItem.PICTURE; // Читаем данные из соответствующего поля.
                    }
                    else
                    {
                        selectItem.PICTURE = null; // Читаем данные из соответствующего поля.
                    }
                    viewModel.UpdateItem(selectItem); // Сохраняем запись в основной коллекции

                    // Данные для подчиненной коллекции
                    if (viewModel.MLCollection.FirstOrDefault(mlc => mlc.TYPEID == viewModel.SelectedJoinItem.TYPEID) != null)
                    {
                        selectMLItem = viewModel.MLCollection.FirstOrDefault(mlc => mlc.TYPEID == viewModel.SelectedJoinItem.TYPEID);
                        selectMLItem.DESCRIPTION = editDESCRIPTION.Text;
                        selectMLItem.NOTE = editNOTE.Text;
                        selectMLItem.LANGUAGE = App.AppLanguage;
                    }

                    // Данные для подчиненной коллекции
                    newMLItem = new BitTypeMLModel
                    {
                        TYPEID = viewModel.SelectedJoinItem.TYPEID,
                        DESCRIPTION = editDESCRIPTION.Text,
                        NOTE = editNOTE.Text,
                        LANGUAGE = App.AppLanguage
                    };
                }
                else
                {
                    // Данные для основной коллекции
                    newItem = new BitTypeModel
                    {
                        TYPENAME = editNAME.Text,
                        PICTURE = viewModel.SelectedJoinItem.PICTURE
                    };
                    viewModel.UpdateItem(newItem); // Создаем запись в основной коллекции

                    // Данные для подчиненной коллекции
                    newMLItem = new BitTypeMLModel
                    {
                        TYPEID = newItem.TYPEID,
                        DESCRIPTION = editDESCRIPTION.Text,
                        NOTE = editNOTE.Text,
                        LANGUAGE = App.AppLanguage
                    };
                }

                viewModel?.UpdateMLItem(newMLItem, selectMLItem); // Создаем запись в подчиненной коллекции
                selectItem = null;
                newItem = null;
                selectMLItem = null;
                newMLItem = null;

                Cancel(); // Отмена изменений в записи.
            }
            catch (Exception ex)
            {
                // Что-то пошло не так
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk);
                });
                return;
            }
        }

        // Отмена изменений.
        private void OnCancel(object sender, EventArgs e)
        {
            try
            {
                Cancel(); // Отмена изменений в записи.
            }
            catch (Exception ex)
            {
                // Что-то пошло не так
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk);
                });
                return;
            }
        }

        #endregion --------- Header - Command --------

        // Обновление записей отображаемых в ListView.
        private void RefreshListView()
        {
            try
            {
                int idItem = -1;
                if (viewModel.SelectedJoinItem != null)
                {
                    idItem = viewModel.SelectedJoinItem.TYPEID;
                }

                if (string.IsNullOrEmpty(SearchBar.Text))
                {
                    SearchBar.Text = string.Empty;
                }

                MasterContent.BeginRefresh();
                viewModel = null;
                BindingContext = null;
                MasterContent.Behaviors.Clear();
                viewModel = new BitType(SearchBar.Text.ToLowerInvariant());
                BindingContext = viewModel;
                MasterContent.EndRefresh();

                if (viewModel.JoinCollection.Count == 0)
                {
                    return;
                }
                else
                {
                    if (NewItem != null)
                    {
                        viewModel.SelectedJoinItem = viewModel?.JoinCollection.FirstOrDefault(temp => temp.TYPEID == viewModel?.JoinCollection.Max(x => x.TYPEID));
                        MasterContent.ScrollTo(viewModel.SelectedJoinItem, ScrollToPosition.Center, true); // Прокручиваем Scroll до активной записи.
                        return;
                    }
                    else if (idItem > 0)
                    {
                        viewModel.SelectedJoinItem = viewModel?.JoinCollection.FirstOrDefault(temp => temp.TYPEID == idItem); // Переходим на последнюю активную запись.
                        MasterContent.ScrollTo(viewModel.SelectedJoinItem, ScrollToPosition.Center, true); // Прокручиваем Scroll до активной записи.
                        return;
                    }
                    else
                    {
                        viewModel.SelectedJoinItem = viewModel.JoinCollection.FirstOrDefault();
                        MasterContent.ScrollTo(viewModel.SelectedJoinItem, ScrollToPosition.Center, true); // Прокручиваем Scroll до активной записи.
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                // Что-то пошло не так
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk);
                });
                return;
            }
        }

        // hardware back button
        protected override bool OnBackButtonPressed()
        {
            base.OnBackButtonPressed();

            try
            {
                if ((viewModel?.DetailMode == false) && (viewModel?.ReadMode == true))
                {
                    Shell.Current.Navigating -= Current_Navigating; // Отписываемся от события Shell.OnNavigating
                    NewItem = null;
                    viewModel = null;
                    Device.BeginInvokeOnMainThread(async () => { await Shell.Current.GoToAsync("..", true); });
                }
                else if ((viewModel?.DetailMode == true) && (viewModel?.ReadMode == false))
                {
                    Cancel(); // Отмена изменений в записи.
                }
                else if (viewModel?.DetailMode == true)
                {
                    viewModel.DetailMode = false;
                    OnSizeChangeInterface(); // Изменение интерфейса при изменении размера окна
                }
            }
            catch { return false; }
            // Always return true because this method is not asynchronous.
            // We must handle the action ourselves: see above.
            return true;
        }

        // Отмена изменений в записи.
        private void Cancel()
        {
            try
            {
                SaveCommandBar.IsVisible = false;
                RefreshListView(); //Обновление записей в ListView Collection
                NewItem = null;
                SearchBar.Focus();
                viewModel.DetailMode = true;
                OnSizeChangeInterface(); // Изменение интерфейса при изменении размера окна
            }
            catch (Exception ex)
            {
                Device.BeginInvokeOnMainThread(async () => { await DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); }); // Что-то пошло не так
                return;
            }
        }

        // Переключение между режимами редактирования и чтения.
        private void GoToEditState()
        {
            try
            {
                viewModel.ReadMode = false;
                viewModel.DetailMode = true;
                SaveCommandBar.IsVisible = true;
                OnSizeChangeInterface(); // Изменение интерфейса при изменении размера окна
            }
            catch (Exception ex)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk);
                });
                return;
            }
        }

        // Изменение интерфейса при изменении размера окна.
        private void OnSizeChangeInterface()
        {
            try
            {
                switch (Device.Idiom)
                {
                    case TargetIdiom.Desktop:
                    case TargetIdiom.Tablet:
                        if (Shell.Current.Width <= 800)
                        {
                            if (viewModel?.DetailMode == false)
                            {
                                // Компактный вид, режим чтения, детали скрыты.
                                Body.ColumnDefinitions[0].Width = GridLength.Star;
                                Body.ColumnDefinitions[1].Width = new GridLength(0);
                                Master.IsVisible = true;
                            }
                            else
                            {
                                // Компактный вид, режим редактирования, только детали (список мастера скрыт).
                                Body.ColumnDefinitions[0].Width = new GridLength(0);
                                Body.ColumnDefinitions[1].Width = GridLength.Star;
                                Master.IsVisible = false;
                            }
                        }
                        else
                        {
                            // Расширенный (полный) вид, режим чтения.
                            Body.ColumnDefinitions[0].Width = new GridLength(320);
                            Body.ColumnDefinitions[1].Width = GridLength.Star;
                            Master.IsVisible = true;
                        }
                        break;

                    case TargetIdiom.Phone:
                        if (viewModel?.DetailMode == false)
                        {
                            // Компактный вид, режим чтения, детали скрыты.
                            Body.ColumnDefinitions[0].Width = GridLength.Star;
                            Body.ColumnDefinitions[1].Width = new GridLength(0);
                            Master.IsVisible = true;
                        }
                        else
                        {
                            // Компактный вид, режим редактирования, только детали (список мастера скрыт).
                            Body.ColumnDefinitions[0].Width = new GridLength(0);
                            Body.ColumnDefinitions[1].Width = GridLength.Star;
                            Master.IsVisible = false;
                        }
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                // Что-то пошло не так
                Device.BeginInvokeOnMainThread(async () => { await DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); });
                return;
            }
        }
    }
}