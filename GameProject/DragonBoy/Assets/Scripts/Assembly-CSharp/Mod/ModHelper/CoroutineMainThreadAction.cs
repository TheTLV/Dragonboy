using System.Collections;
using UnityEngine;

namespace Mod.ModHelper
{
    /// <summary>
    /// Hành động lặp lại theo chu kỳ, chạy bằng coroutine trên main thread của Unity.
    /// </summary>
    /// <remarks>
    /// Ported from EdwardLTC/Dragonboy. Khác bản gốc: instance được tự tạo khi truy cập <see cref="gI"/> lần đầu,
    /// nên không cần ModScript để khởi tạo.
    /// </remarks>
    internal abstract class CoroutineMainThreadAction<T> : MonoBehaviour
        where T : CoroutineMainThreadAction<T>
    {
        static T instance;

        Coroutine runningCoroutine;

        internal static T gI
        {
            get
            {
                if (instance == null)
                {
                    GameObject gameObject = new GameObject(typeof(T).Name);
                    DontDestroyOnLoad(gameObject);
                    instance = gameObject.AddComponent<T>();
                }
                return instance;
            }
        }

        /// <summary>
        /// Khoảng thời gian (giây) giữa 2 lần gọi <see cref="OnUpdate"/>.
        /// </summary>
        protected abstract float Interval { get; }

        internal bool IsActing { get; private set; }

        protected virtual void Awake()
        {
            if (instance == null)
                instance = (T)this;
            else if (instance != this)
                Destroy(gameObject);
        }

        protected abstract IEnumerator OnUpdate();

        protected virtual void OnStart() { }

        protected virtual void OnStop() { }

        internal void Toggle(bool value) => Toggle((bool?)value);

        internal void Toggle(bool? value = null)
        {
            if (value == null && IsActing || value == false)
                StopAction();
            else if (value == null && !IsActing || value == true)
                StartAction();
        }

        void StartAction()
        {
            if (IsActing)
                return;
            IsActing = true;
            OnStart();
            runningCoroutine = StartCoroutine(Run());
        }

        void StopAction()
        {
            if (!IsActing)
                return;
            IsActing = false;
            if (runningCoroutine != null)
            {
                StopCoroutine(runningCoroutine);
                runningCoroutine = null;
            }
            OnStop();
        }

        IEnumerator Run()
        {
            while (IsActing)
            {
                yield return StartCoroutine(OnUpdate());
                yield return new WaitForSecondsRealtime(Interval);
            }
        }
    }
}
