using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Selection;
using HanumanInstitute.MvvmDialogs;
using ReactiveUI;
using TruthFinder.Views;

namespace TruthFinder.ViewModels;

public class SelectModelViewModel : ViewModelBase, IModalDialogViewModel
{
    public static readonly string modelsPath = Path.Combine(Directory.GetCurrentDirectory(), "model");

    public SelectModelViewModel()
    {
        LoadData();
    }

    private void LoadData()
    {
        Models = new ObservableCollection<ModelData>();

        if (!Directory.Exists(modelsPath))
        {
            Directory.CreateDirectory(modelsPath);
        }

        Models.Add(new ModelData()
        {
            IsExist = false, Name = "saiga2_7b_gguf_model-q2_K.gguf", Requirements = "6GB RAM",
            Url = "https://huggingface.co/IlyaGusev/saiga2_7b_gguf/resolve/main/model-q2_K.gguf"
        });
        Models.Add(new ModelData()
        {
            IsExist = false, Name = "saiga2_7b_gguf_model-q3_K.gguf", Requirements = "7GB RAM",
            Url = "https://huggingface.co/IlyaGusev/saiga2_7b_gguf/resolve/main/model-q3_K.gguf"
        });
        Models.Add(new ModelData()
        {
            IsExist = false, Name = "saiga2_7b_gguf_model-q4_K.gguf", Requirements = "8GB RAM",
            Url = "https://huggingface.co/IlyaGusev/saiga2_7b_gguf/resolve/main/model-q4_K.gguf"
        });
        Models.Add(new ModelData()
        {
            IsExist = false, Name = "saiga2_7b_gguf_model-q5_K.gguf", Requirements = "9GB RAM",
            Url = "https://huggingface.co/IlyaGusev/saiga2_7b_gguf/resolve/main/model-q5_K.gguf"
        });
        Models.Add(new ModelData()
        {
            IsExist = false, Name = "saiga2_7b_gguf_model-q8_0.gguf", Requirements = "10 RAM",
            Url = "https://huggingface.co/IlyaGusev/saiga2_7b_gguf/resolve/main/model-q8_0.gguf"
        });


        Models.Add(new ModelData()
        {
            IsExist = false, Name = "saiga2_13b_gguf_model-q2_K.gguf", Requirements = "6GB RAM",
            Url = "https://huggingface.co/IlyaGusev/saiga2_13b_gguf/resolve/main/model-q2_K.gguf"
        });
        Models.Add(new ModelData()
        {
            IsExist = false, Name = "saiga2_13b_gguf_model-q3_K.gguf", Requirements = "8GB RAM",
            Url = "https://huggingface.co/IlyaGusev/saiga2_13b_gguf/resolve/main/model-q3_K.gguf"
        });
        Models.Add(new ModelData()
        {
            IsExist = false, Name = "saiga2_13b_gguf_model-q4_K.gguf", Requirements = "10GB RAM",
            Url = "https://huggingface.co/IlyaGusev/saiga2_13b_gguf/resolve/main/model-q4_K.gguf"
        });
        Models.Add(new ModelData()
        {
            IsExist = false, Name = "saiga2_13b_gguf_model-q5_K.gguf", Requirements = "14GB RAM",
            Url = "https://huggingface.co/IlyaGusev/saiga2_13b_gguf/resolve/main/model-q5_K.gguf"
        });
        Models.Add(new ModelData()
        {
            IsExist = false, Name = "saiga2_13b_gguf_model-q8_0.gguf", Requirements = "16GB RAM",
            Url = "https://huggingface.co/IlyaGusev/saiga2_13b_gguf/resolve/main/model-q8_0.gguf"
        });


        string selectedModelName = string.Empty;

        var settings = Path.Combine(modelsPath, "settings.file");
        if (File.Exists(settings))
        {
            using (var file = File.OpenText(settings))
            {
                selectedModelName = file.ReadLine();
            }
        }

        if (!Directory.Exists(modelsPath))
        {
            Directory.CreateDirectory(modelsPath);
        }

        var files = Directory.EnumerateFiles(modelsPath, "*.gguf");

        foreach (var file in files)
        {
            var name = Path.GetFileName(file);
            if (Models.All(var => var.Name != name))
            {
                Models.Add(new ModelData()
                    { IsExist = true, Name = name, Requirements = string.Empty, Url = String.Empty });
            }
            else
            {
                Models.First(var => var.Name == name).IsExist = true;
                Models.First(var => var.Name == name).IsSelected = true;
            }
        }

        if (Models.Any(var => var.Name == selectedModelName))
        {
            SelectedModel = Models.First(var => var.Name == selectedModelName);
            SelectedModel.IsSelected = true;
        }
        else
        {
            if (Models.Any(var => var.IsExist))
            {
                SelectedModel = Models.First(var => var.IsExist);
                SelectedModel.IsSelected = true;
            }
            else
            {
                SelectedModel =  Models.First();
                SelectedModel.IsSelected = true;
            }
        }
    }


    public bool? DialogResult { get; }

    public ObservableCollection<ModelData> Models { get; set; }
    private ModelData _SelectedModel;

    public ModelData SelectedModel
    {
        get { return _SelectedModel; }
        set
        {
            foreach (var modelData in Models)
            {
                modelData.IsSelected = false;
            }

            if (value != null)
            {
                value.IsSelected = true;

                if (File.Exists(Path.Combine(modelsPath, "settings.file")))
                {
                    File.Delete(Path.Combine(modelsPath, "settings.file"));
                }

                using (var file = File.CreateText(Path.Combine(modelsPath, "settings.file")))
                {
                    file.Write(value.Name);
                }
                this.RaisePropertyChanged("IsExist");
            }
            this.RaiseAndSetIfChanged(ref _SelectedModel, value);
        }
    }

    private bool _IsLoading = false;

    public bool IsLoading
    {
        get { return _IsLoading; }
        set { this.RaiseAndSetIfChanged(ref _IsLoading, value); }
    }

    private double _ProgessValue = 0;

    public double ProgessValue
    {
        get { return _ProgessValue; }
        set { this.RaiseAndSetIfChanged(ref _ProgessValue, value); }
    }
    
    public bool? IsExist
    {
        get { return SelectedModel?.IsSelected; }
    }
    public void DeleteAll()
    {
        var files = Directory.EnumerateFiles(modelsPath, "*.gguf");
        var file = Path.Combine(modelsPath, "settings.file");
        File.Delete(file);

        foreach (var f in files)
        {
            File.Delete(f);
        }

        LoadData();
    }

    public async void Download()
    {
        IsLoading = true;
        ProgessValue = 0;
        if (!SelectedModel.IsExist)
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                    await wc.DownloadFileTaskAsync(
                        new System.Uri(SelectedModel.Url),
                        Path.Combine(modelsPath, SelectedModel.Name)
                    );
                }

                SelectedModel.IsExist = true;
            }
            catch (Exception e)
            {
                
            }

        }
        IsLoading = false;
    }

    void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        var progress = (double)e.BytesReceived / (double)e.TotalBytesToReceive * 100;
        ProgessValue = progress;
    }

    public async void Close()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
            lifetime.Windows.First(var => var.DataContext == this).Close();
    }
}

public class ModelData : ViewModelBase
{
    public string Url { get;  set; }
    public string Name { get; set; }
    public string Requirements { get; set; }
    public bool IsExist { get; set; }
    public bool IsSelected { get; set; }
}