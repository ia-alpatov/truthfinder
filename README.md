# TruthFinder - cила в правде
Приложение для поиска общего и разного в полярных СМИ

Проверено на Ubuntu 23.10


## Как запустить приложени?

Выполняем:
```
wget https://huggingface.co/IlyaGusev/saiga2_7b_gguf/resolve/main/model-q4_K.gguf
git clone https://github.com/ggerganov/llama.cpp
cp model-q4_K.gguf llama.cpp/models/model-q4_K.gguf
cd llama.cpp
make
./main -m ./models/model-q4_K.gguf -p "Вопрос: Почему трава зеленая? Ответ:" -n 512 --temp 0.1
```

Копируем папку llama.cpp в папку с исполняемым файлом и запускаем

## Как пользоваться приложением?
* Загружаем выбранную модель
* Вводим в поля, для ссылок адреса статей, для поддерживаемых сайтов СМИ
* Выбираем или сами вводим промты
* Жмём на "Получить ответ"
