namespace IdSrv.Account.WebApi.RestClient
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Набор функций-помощников для доступа к WebApi через сгенерированного nswag-ос rest-клиента.
    /// </summary>
    public static class RestApiHelpers
    {
        /// <summary>
        /// Вызвать метод, ошибочный код которого интерпретируется как false (например, 404 - пользователь
        /// не найден).
        /// </summary>
        /// <param name="callback">Вызов метода сгенерированного rest-клиента.</param>
        /// <returns>true, если метод вернул успешной значение (2xx), иначе false.</returns>
        public static async Task<bool> CallBoolApi(Func<Task> callback)
        {
            try
            {
                await callback();
                return true;
            }
            catch (RestClientException)
            {
                return false;
            }
        }

        /// <summary>
        /// Вызвать метод, который должен вернуть dto. Если rest сервис вернул ошибочный код (например, 404),
        /// то вернуть false (если такой ошибочный код подразумевается работой сервиса).
        /// </summary>
        /// <param name="callback">Вызов метода сгенерированного rest-клиента.</param>
        /// <returns>
        /// Значение типа <typeparamref name="T"/>, если метод вернул успешной значение (2xx), иначе 
        /// дефолтное значение для <typeparamref name="T"/> (например, null для ссылочных типов).
        /// </returns>
        /// <typeparam name="T">Тип dto, которое возвращается методом rest-сервиса.</typeparam>
        public static async Task<T> CallValueApi<T>(Func<Task<T>> callback)
        {
            try
            {
                return await callback();
            }
            catch (RestClientException)
            {
                return default(T);
            }
        }
    }
}