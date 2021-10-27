using BITIDAPP.Models.Bits;
using BITIDAPP.Resources;
using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BITIDAPP.Views.Bits
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BitCodePage : ContentPage
    {
        private BitCode viewModel;

        private readonly string FilterTypeID = null; // Переменая фильтра по типу.
        private BitCodeModel NewItem = null; // Новая запись.

        public BitCodePage()
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
                        viewModel = new BitCode(null, string.Empty);
                    }

                    BindingContext = viewModel;

                    if (viewModel.SelectedJoinItem == null) // Если не открыт Picer для выбора картинки в Android
                    {
                        viewModel.SelectedJoinItem = viewModel?.Collection?.FirstOrDefault(); // Переходим на первую запись.
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
                    picTYPENAME.SelectedIndex = viewModel.TypePickerList.IndexOf(viewModel.TypePickerList.Where(X => X.TYPEID == viewModel?.SelectedJoinItem.TYPEID).FirstOrDefault());
                    editSERIAL.SelectedIndex = viewModel.SerialList.IndexOf(viewModel.SerialList.Where(X => X.SERIALS == viewModel?.SelectedJoinItem.SERIAL).FirstOrDefault());

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
                        if (Shell.Current.Height <= 800)
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
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await DisplayAlert(AppResource.messageAttention, AppResource.messageNoActiveRecord, AppResource.messageСancel);
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
                viewModel?.Collection?.Add(viewModel?.NewItem);
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
                        int indexItem = viewModel.Collection.IndexOf(viewModel.SelectedJoinItem);
                        // Удаляем текущую запись.
                        viewModel?.DeleteItem();

                        if (viewModel?.Collection?.FirstOrDefault() != null) // Если в Collection есть записи.
                        {
                            if (indexItem == 0) // Если текущая запись первая.
                            {
                                viewModel.SelectedJoinItem = viewModel?.Collection?[indexItem]; // Переходим на следующую запись после удаленной, у которой такой же индекс как и у удаленной.
                                return;
                            }
                            else
                            {
                                viewModel.SelectedJoinItem = viewModel?.Collection[indexItem - 1]; // Переходим на предыдующую запись перед удаленной.
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
                BitCodeModel selectItem = viewModel?.Collection.FirstOrDefault(temp => temp.BITCODEID == viewModel?.SelectedJoinItem.BITCODEID);
                selectItem.TYPEID = viewModel.TypePickerList[picTYPENAME.SelectedIndex].TYPEID;
                selectItem.SERIAL = viewModel.SerialList[editSERIAL.SelectedIndex].SERIALS; // Читаем данные из соответствующего поля.
                selectItem.SYMBOL = editSYMBOL.Text; // Читаем данные из соответствующего поля.
                selectItem.FEATURE = editFEATURE.Text; // Читаем данные из соответствующего поля.
                selectItem.SPECIFICATION = editSPECIFICATION.Text; // Читаем данные из соответствующего поля.
                selectItem.LANGUAGE = App.AppLanguage; // Читаем данные из соответствующего поля.

                viewModel?.UpdateItem(selectItem); // Сохраняем или создаем новую запись в подчиненной коллекции

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

        #endregion

        // Обновление записей отображаемых в ListView.
        private void RefreshListView()
        {
            try
            {
                int idItem = -1;
                if (viewModel.SelectedJoinItem != null)
                {
                    idItem = viewModel.SelectedJoinItem.BITCODEID;
                }

                if (string.IsNullOrEmpty(SearchBar.Text))
                {
                    SearchBar.Text = string.Empty;
                }

                MasterContent.BeginRefresh();
                viewModel = null;
                BindingContext = null;
                MasterContent.Behaviors.Clear();
                viewModel = new BitCode(FilterTypeID, SearchBar.Text.ToLowerInvariant());
                BindingContext = viewModel;
                MasterContent.EndRefresh();

                if (viewModel.Collection.Count == 0)
                {
                    return;
                }
                else
                {
                    if (NewItem != null)
                    {
                        viewModel.SelectedJoinItem = viewModel?.Collection.FirstOrDefault(temp => temp.BITCODEID == viewModel?.Collection.Max(x => x.BITCODEID));
                        return;
                    }
                    else if (idItem > 0)
                    {
                        viewModel.SelectedJoinItem = viewModel?.Collection.FirstOrDefault(temp => temp.BITCODEID == idItem); // Переходим на последнюю активную запись.
                        return;
                    }
                    else
                    {
                        viewModel.SelectedJoinItem = viewModel.Collection.FirstOrDefault();
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
                // Что-то пошло не так
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk);
                });
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
                        if (Shell.Current.Height <= 800)
                        {
                            if (viewModel?.DetailMode == false)
                            {
                                // Компактный вид, режим чтения, детали скрыты.
                                Body.RowDefinitions[0].Height = new GridLength(0);
                                Body.RowDefinitions[1].Height = GridLength.Star;
                                Master.IsVisible = true;
                            }
                            else
                            {
                                // Компактный вид, режим редактирования, только детали (список мастера скрыт).
                                Body.RowDefinitions[0].Height = GridLength.Star;
                                Body.RowDefinitions[1].Height = new GridLength(0);
                                Master.IsVisible = false;
                            }
                        }
                        else
                        {
                            // Расширенный (полный) вид, режим чтения.
                            Body.RowDefinitions[0].Height = GridLength.Auto;
                            Body.RowDefinitions[1].Height = GridLength.Star;
                            Master.IsVisible = true;
                        }
                        break;

                    case TargetIdiom.Phone:
                        if (viewModel?.DetailMode == false)
                        {
                            // Компактный вид, режим чтения, детали скрыты.
                            Body.RowDefinitions[0].Height = new GridLength(0);
                            Body.RowDefinitions[1].Height = GridLength.Star;
                            Master.IsVisible = true;
                        }
                        else
                        {
                            // Компактный вид, режим редактирования, только детали (список мастера скрыт).
                            Body.RowDefinitions[0].Height = GridLength.Star;
                            Body.RowDefinitions[1].Height = new GridLength(0);
                            Master.IsVisible = false;
                        }
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
    }
}