using BITIDAPP.Models.Bits;
using BITIDAPP.Resources;
using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BITIDAPP.Views.Bits
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BitODPage : ContentPage
    {
        private BitOD viewModel;
        private BitODModel NewItem = null; // Новая запись.

        public BitODPage()
        {
            InitializeComponent();
        }

        // События непосредственно перед тем как страница становится видимой.
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                indicator.IsRunning = true;
                IsBusy = true; ;  // Затеняем задний фон и запускаем ProgressRing

                if (viewModel == null) // Если не открыт Picer для выбора картинки в Android
                {
                    viewModel = new BitOD(string.Empty);
                }

                BindingContext = viewModel;

                if (viewModel.SelectedJoinItem == null) // Если не открыт Picer для выбора картинки в Android
                {
                    viewModel.SelectedJoinItem = viewModel?.Collection?.FirstOrDefault(); // Переходим на первую запись.
                }

                IsBusy = false;
                indicator.IsRunning = false;
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); // Что-то пошло не так
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

        // События непосредственно перед тем как страница становится видимой.
        private void OnSelection(object sender, SelectedItemChangedEventArgs e) // Загружаем дочернюю форму и передаем ей параметр ID.
        {
            try
            {
                if (e.SelectedItem != null) // Если в Collection есть записи.
                {
                    EditButton.IsEnabled = true; // Кнопка Редактирования активна.
                    DeleteButton.IsEnabled = true; // Кнопка Удаления записи активна.
                    //Master.ScrollTo(viewModel.SelectedJoinItem, ScrollToPosition.Center, true); // Прокручиваем Scroll до активной записи.
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
                    await Application.Current.MainPage.DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk);
                });
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
                    await Application.Current.MainPage.DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk);
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

                if (Master.SelectedItem != null)
                {
                    GoToEditState(); // Переходим в режим редактирования.
                }
                else
                {
                    EditButton.IsEnabled = false; // Если нет активной записи в MasterListView, кнопка Редактирования неактивна.
                    Device.BeginInvokeOnMainThread(async () => { await DisplayAlert(AppResource.messageAttention, AppResource.messageNoActiveRecord, AppResource.messageСancel); }); // Что-то пошло не так
                    return;
                }
            }
            catch (Exception ex)
            {
                // Что-то пошло не так
                Device.BeginInvokeOnMainThread(async () => { await DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk); });
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
                Master.ScrollTo(viewModel.SelectedJoinItem, ScrollToPosition.Center, true); // Прокручиваем Scroll до активной записи.

                GoToEditState(); // Переходим в режим редактирования.
            }
            catch (Exception ex)
            {
                // Что-то пошло не так
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk);
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
                    bool dialog = await Application.Current.MainPage.DisplayAlert(AppResource.messageTitleAction, AppResource.messageDelete, AppResource.messageOk, AppResource.messageСancel);
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
                    await Application.Current.MainPage.DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk);
                });
                return;
            }
        }

        // Сохраняем изменения.
        private void OnSave(object sender, EventArgs e)
        {
            try
            {
                BitODModel selectItem = viewModel?.Collection.FirstOrDefault(temp => temp.BITODID == viewModel?.SelectedJoinItem.BITODID);
                if (Master.SelectedItem != null)
                {
                    selectItem.BITOD = decimal.Parse(editBITOD.Text); // Читаем данные из соответствующего поля.
                    selectItem.BITODINCH = editBITODINCH.Text; // Читаем данные из соответствующего поля.
                    selectItem.DESCRIPTION = editDESCRIPTION.Text; // Читаем данные из соответствующего поля.

                    // Сохраняем или создаем новую запись в основной коллекции
                    viewModel?.UpdateItem(selectItem);

                    Cancel(); // Отмена изменений в записи.
                }
                else
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await Application.Current.MainPage.DisplayAlert(AppResource.messageAttention, AppResource.messageNewItem, AppResource.messageСancel);
                    });
                    return;
                }
            }
            catch (Exception ex)
            {
                // Что-то пошло не так
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk);
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
                    await Application.Current.MainPage.DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk);
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
                    idItem = viewModel.SelectedJoinItem.BITODID;
                }

                if (string.IsNullOrEmpty(SearchBar.Text))
                {
                    SearchBar.Text = string.Empty;
                }

                Master.BeginRefresh();
                viewModel = null;
                BindingContext = null;
                Master.Behaviors.Clear();
                viewModel = new BitOD(SearchBar.Text.ToLowerInvariant());
                BindingContext = viewModel;
                Master.EndRefresh();

                if (viewModel.Collection.Count == 0)
                {
                    return;
                }
                else
                {
                    if (NewItem != null)
                    {
                        viewModel.SelectedJoinItem = viewModel?.Collection.FirstOrDefault(temp => temp.BITODID == viewModel?.Collection.Max(x => x.BITODID));
                        return;
                    }
                    else if (idItem > 0)
                    {
                        viewModel.SelectedJoinItem = viewModel?.Collection.FirstOrDefault(temp => temp.BITODID == idItem); // Переходим на последнюю активную запись.
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
                    await Application.Current.MainPage.DisplayAlert(AppResource.messageError, ex.Message, AppResource.messageOk);
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
                if (viewModel?.ReadMode == true)
                {
                    Shell.Current.Navigating -= Current_Navigating; // Отписываемся от события Shell.OnNavigating
                    NewItem = null;
                    viewModel = null;
                    Device.BeginInvokeOnMainThread(async () => { await Shell.Current.GoToAsync("..", true); });
                }
                else
                {
                    Cancel(); // Отмена изменений в записи.
                }
            }
            catch { return false; }
            // Always return true because this method is not asynchronous. We must handle the action ourselves: see above.
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
                //Detail.Focus();
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
                SaveCommandBar.IsVisible = true;
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
    }
}