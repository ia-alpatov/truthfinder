using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using HanumanInstitute.MvvmDialogs;
using ReactiveUI;
using TruthFinder.Parsers;
using TruthFinder.Views;

namespace TruthFinder.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly IDialogService dialogService;

    private string llamaCppPath = Path.Combine(Directory.GetCurrentDirectory(), "llama.cpp", Environment.OSVersion.Platform ==  PlatformID.Unix ? "main":"main.exe");
    
    private List<INewsParser> newsParsers = new List<INewsParser>()
    {
        new  MeduzaNewsParser(),
        new GazetaRuNewsParser()
    };
    public MainWindowViewModel(IDialogService dialogService)
    {
        this.dialogService = dialogService;
        PromptText = ReadyToUsePrompts.First();
        SelectedPrompt = ReadyToUsePrompts.First();
    }

    private string _Output;
    public string Output
    {
        get { return _Output; }
        set
        {
            this.RaiseAndSetIfChanged(ref _Output, value);
        }
    }
    private string _InputLeft;
    public string InputLeft
    {
        get { return _InputLeft; }
        set
        {
            this.RaiseAndSetIfChanged(ref _InputLeft, value);
        }
    }

    private string _InputRight;
    public string InputRight
    {
        get { return _InputRight; }
        set
        {
            this.RaiseAndSetIfChanged(ref _InputRight, value);
        }
    }
    
    private List<string> _ReadyToUsePrompts = new List<string>()
    {
        "Ты система для определеления наиболее похожего на пропаганду и пиара текста и вывода в виде ответа цифру 1 или 2, где 1 - первый текст наиболее пропагандисткий, а 2 - второй текст наиболее пропагандисткий, одного из двух текстов СМИ с разными политическими взглядами на одну тему, которые разделены знаком | Вопрос: Какой наиболее похож на пропаганду текси из двух текстов далее, выведи номер, которые разделеныя символом '|'?  ",
        "Ты система для определеления политической ориентации в каждом из двух текстах на одну тему, которые разделены знаком |  Вопрос: Какой политической ориентации первый и второй текст? ",
        "Ты система для определеления наиболее упаднического настроения, одного из двух текстов СМИ с разными политическими взглядами на одну тему, в виде ответа цифру 1 или 2, где 1 - первый текст наиболее упаднический, а 2 - второй текст наиболее упаднический, которые разделены знаком |  Вопрос: Какой наиболее упаднический текст из двух текстов далее, которые разделеныя символом '|'? ",
        "Ты система для определеления какой текст был бы полезен для развития патриотизма в России, первый текст наиболее полезен или второй текст наиболее полезен, которые разделены знаком |  Вопрос: Какой наиболее полезный для патриотизма текст из двух текстов далее, которые разделеныя символом '|'? ",
        
    };
    public List<string> ReadyToUsePrompts
    {
        get { return _ReadyToUsePrompts; }
        set
        {
            this.RaiseAndSetIfChanged(ref _ReadyToUsePrompts, value);
        }
    }
    
    private string _SelectedPrompt;
    public string SelectedPrompt
    {
        get { return _SelectedPrompt; }
        set
        {
            this.RaiseAndSetIfChanged(ref _SelectedPrompt, value);
            PromptText = value;
        }
    }
    private string _PromtText = string.Empty;
    public string PromptText
    {
        get { return _PromtText; }
        set
        {
            this.RaiseAndSetIfChanged(ref _PromtText, value);
        }
    }
    
    private string _InputUrlLeft;
    public string InputUrlLeft
    {
        get { return _InputUrlLeft; }
        set
        {
            this.RaiseAndSetIfChanged(ref _InputUrlLeft, value);
            if (!string.IsNullOrWhiteSpace(value))
            {
                foreach (var parser in newsParsers)
                {
                    if (parser.IsUrlRight(value))
                    {
                        InputLeft = parser.GetTextByUrl(value);
                        InputLeft = InputLeft.Replace("\u00a0", " ");
                        break;
                    }
                }
            }
        }
    }
    private string _InputUrlRight;
    public string InputUrlRight
    {
        get { return _InputUrlRight; }
        set
        {
            this.RaiseAndSetIfChanged(ref _InputUrlRight, value);
            if (!string.IsNullOrWhiteSpace(value))
            {
                foreach (var parser in newsParsers)
                {
                    if (parser.IsUrlRight(value))
                    {
                        InputRight = parser.GetTextByUrl(value);
                        InputRight = InputRight.Replace("\u00a0", " ");
                        break;
                    }
                }
            }
            
        }
    }
    
    private string _ModelName;
    public string ModelName
    {
        get { return _ModelName; }
        set
        {
            this.RaiseAndSetIfChanged(ref _ModelName, value);
        }
    }

    public async void ChooseModel()
    {
        await dialogService.ShowModelChooser(this);
    }
    
    private bool _IsLoading = false;

    public bool IsLoading
    {
        get { return _IsLoading; }
        set { this.RaiseAndSetIfChanged(ref _IsLoading, value); }
    }

    
    public async void RunLLM()
    {
        IsLoading = true;
        try
        {
            if (string.IsNullOrWhiteSpace(PromptText))
            {
                await dialogService.ShowMessageBoxAsync(this, "Заполните текст задачи или выберите из готовых",
                    "Ошибка!");
                return;
            }

            if (string.IsNullOrWhiteSpace(InputLeft) || string.IsNullOrWhiteSpace(InputRight))
            {
                await dialogService.ShowMessageBoxAsync(this, "Введите тексты", "Ошибка!");
                return;
            }

            if (File.Exists(Path.Combine(SelectModelViewModel.modelsPath, "settings.file")))
            {
                string modelName =
                    await File.ReadAllTextAsync(Path.Combine(SelectModelViewModel.modelsPath, "settings.file"));

                if (File.Exists(Path.Combine(SelectModelViewModel.modelsPath, modelName)))
                {
                    if (File.Exists(llamaCppPath))
                    {
                        try
                        {
                            Process p = new Process();
                            p.StartInfo.UseShellExecute = false;
                            p.StartInfo.FileName = llamaCppPath;    
                            p.StartInfo.RedirectStandardOutput = true;
                            p.StartInfo.RedirectStandardError = true;
                            var prompt = SelectedPrompt.Split("Вопрос:");
                     
                            p.StartInfo.Arguments = $@"-m {Path.Combine(SelectModelViewModel.modelsPath,modelName)} -p ""Задание: {(prompt.Length == 2 ? prompt[0] + Environment.NewLine + "Вопрос: " + prompt[1]: prompt) }  {InputLeft.Trim().Replace(Environment.NewLine, " ")} | {InputRight.Trim().Replace(Environment.NewLine, " ")} {Environment.NewLine}Ответ:"" -n 512 --temp 0.1 -t 6 -b 256 -c 2048";
                            p.Start();
                       
                            var output = await p.StandardOutput.ReadToEndAsync();
                            
                            p.WaitForExit();
                            
                            if (string.IsNullOrWhiteSpace(output) || !output.Contains("Ответ: "))
                            {
                                await dialogService.ShowMessageBoxAsync(this, "При работе LLM произошла ошибка", "Ошибка!");
                            }
                            else
                            {
                                var answer = output.Split("Ответ: ").Last().Replace("bot",string.Empty);
                                Output = answer;
                            }
                        }
                        catch (Exception e)
                        {
                            await dialogService.ShowMessageBoxAsync(this, $"При работе LLM произошла ошибка: {e}", "Ошибка!");
                        }
                    }
                    else
                    {
                        await dialogService.ShowMessageBoxAsync(this, "Проверьте наличие Llama.cpp", "Ошибка!");
                    }
                }
                else
                {
                    await dialogService.ShowMessageBoxAsync(this, "Модель не существует. Выберите модель и скачайте её",
                        "Ошибка!");
                }
            }
            else
            {
                await dialogService.ShowMessageBoxAsync(this, "Модель не существует. Выберите модель и скачайте её",
                    "Ошибка!");
            }
        }
        finally
        {
            IsLoading = false;
        }
    }
}