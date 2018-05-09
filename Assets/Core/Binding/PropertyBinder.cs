﻿using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace BridgeUI.Binding
{
    public class PropertyBinder
    {
        public IBindingContext Context { get; private set; }
        public ViewModelBase viewModel { get; private set; }

        protected event UnityAction<ViewModelBase> binders;
        protected event UnityAction<ViewModelBase> unbinders;

        public PropertyBinder(IBindingContext context)
        {
            this.Context = context;
        }

        public void Bind(ViewModelBase viewModel)
        {
            //Debug.Log("Bind:" + viewModel);
            this.viewModel = viewModel;

            if (viewModel != null && binders != null)
            {
                binders.Invoke(viewModel);
                viewModel.OnBinding(Context);
            }
        }

        public void Unbind()
        {
            //Debug.Log("UnBind:" + viewModel);
            if (viewModel != null && unbinders != null)
            {
                unbinders.Invoke(viewModel);
                viewModel.OnUnBinding(Context);
            }
            this.viewModel = null;
        }
        /// <summary>
        /// 利用反射自动完成绑定
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="memberPath"></param>
        /// <param name="sourceName"></param>
        public void RegistMember<T>(string memberPath, string sourceName)
        {
            object root = Context;
            var member = GetDeepMember(ref root, memberPath);
            UnityAction<T> onViewModelChanged = (value) =>
            {
                SetMemberValue<T>(root, member, value);
            };
            RegistValueCharge(onViewModelChanged, sourceName);
        }
        /// <summary>
        /// 注册通用事件
        /// </summary>
        /// <param name="uEvent"></param>
        /// <param name="sourceName"></param>
        public virtual void RegistEvent(UnityEvent uEvent, string sourceName)
        {
            UnityAction action = () =>
            {
                var prop = viewModel.GetUsefulBindTarget<C_PanelAction>(sourceName);
                if (prop != null && prop.Target != null)
                {
                    var func = prop.Target as PanelAction;
                    func.Invoke(Context);
                }
                else
                {
                    Debug.LogWarningFormat("target prop of {0} not exist in {1}", sourceName, viewModel);
                }
            };

            binders += viewModel =>
            {
                uEvent.AddListener(action);
            };

            unbinders += viewModel =>
            {
                uEvent.RemoveListener(action);
            };
        }
        /// <summary>
        /// 注册通用事件
        /// </summary>
        /// <param name="uEvent"></param>
        /// <param name="sourceName"></param>
        public virtual void RegistEvent<T>(UnityEvent uEvent, string sourceName, T target)
        {
            UnityAction action = () =>
            {
                var prop = viewModel.GetUsefulBindTarget<C_PanelAction<T>>(sourceName);
                if (prop != null && prop.Target != null)
                {
                    var func = prop.Target as PanelAction<T>;
                    func.Invoke(Context, target);
                }
                else
                {
                    Debug.LogWarningFormat("target prop of {0} not exist in {1}", sourceName, viewModel);
                }

            };

            binders += viewModel =>
            {
                uEvent.AddListener(action);
            };

            unbinders += viewModel =>
            {
                uEvent.RemoveListener(action);
            };
        }

        /// <summary>
        /// 注册事件
        /// (其中arguments中的参数只能是引用类型,否则无法正常显示)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uEvent"></param>
        /// <param name="sourceName"></param>
        public virtual void RegistEvent<T, P>(UnityEvent<T> uEvent, string sourceName, P target)
        {
            UnityAction<T> action = (x) =>
            {
                var prop = viewModel.GetUsefulBindTarget<C_PanelAction<P>>(sourceName);
                if (prop != null && prop.Target != null)
                {
                    var func = prop.Target as PanelAction<P>;
                    func.Invoke(Context, target);
                }
                else
                {
                    Debug.LogWarningFormat("target prop of {0} not exist in {1}", sourceName, viewModel);
                }
            };

            binders += viewModel =>
            {
                uEvent.AddListener(action);
            };

            unbinders += viewModel =>
            {
                uEvent.RemoveListener(action);
            };
        }

        /// <summary>
        /// 手动指定绑定事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceName"></param>
        /// <param name="onViewModelChanged"></param>
        public void RegistValueCharge<T>(UnityAction<T> onViewModelChanged, string sourceName)
        {
            binders += (viewModel) =>
            {
                var prop = viewModel.GetUsefulBindTarget<B_Property<T>>(sourceName);
                if (onViewModelChanged != null && prop != null)
                {
                    onViewModelChanged.Invoke(prop.Value);
                    prop.RegistValueChanged(onViewModelChanged);
                }
            };

            unbinders += (viewModel) =>
            {
                var prop = viewModel.GetUsefulBindTarget<B_Property<T>>(sourceName);
                if (onViewModelChanged != null && prop != null)
                {
                    prop.RemoveValueChanged(onViewModelChanged);
                }
            };
        }

        /// <summary>
        ///  Get Member Value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Instance"></param>
        /// <param name="temp"></param>
        /// <returns></returns>
        protected static T GetMemberValue<T>(object Instance, MemberInfo temp)
        {
            if (temp == null)
            {
                return default(T);
            }

            if (temp is FieldInfo)
            {
                return (T)(temp as FieldInfo).GetValue(Instance);
            }
            else if (temp is PropertyInfo)
            {
                return (T)(temp as PropertyInfo).GetValue(Instance, null);
            }
            else
            {
                return (T)(temp as MethodInfo).Invoke(Instance, null);
            }
        }

        /// <summary>
        /// Set Member Value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Instance"></param>
        /// <param name="temp"></param>
        /// <param name="value"></param>
        protected static void SetMemberValue<T>(object Instance, MemberInfo temp, T value)
        {
            if (temp == null)
            {
                return;
            }

            if (temp is FieldInfo)
            {
                (temp as FieldInfo).SetValue(Instance, value);
            }
            else if (temp is PropertyInfo)
            {
                (temp as PropertyInfo).SetValue(Instance, value, null);
            }
            else
            {
                (temp as MethodInfo).Invoke(Instance, new object[] { value });
            }
        }

        /// <summary>
        /// Invoke Method Value
        /// </summary>
        /// <param name="memberName"></param>
        protected static void InvokeMethod(object Instance, string memberName, params object[] value)
        {
            Debug.Log(Instance + ":" + memberName);
            var temps = Instance.GetType().GetMember(memberName);
            if (temps.Length > 0)
            {
                var temp = temps[0];
                if (temp is MethodInfo)
                {
                    (temp as MethodInfo).Invoke(Instance, value);
                }
            }
        }

        /// <summary>
        /// 深度获取对象
        /// </summary>
        /// <param name="Instance"></param>
        /// <param name="memberName"></param>
        /// <returns></returns>
        protected static MemberInfo GetDeepMember(ref object Instance, string memberName)
        {
            var names = memberName.Split(new char[] { '.' });
            Type type = Instance.GetType();
            MemberInfo member = null;
            for (int i = 0; i < names.Length; i++)
            {
                var members = type.GetMember(names[i], BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (members == null || members.Length == 0)
                {
                    return null;
                }
                else
                {
                    member = members[0];

                    if (member is FieldInfo)
                    {
                        var fieldInfo = (member as FieldInfo);
                        type = fieldInfo.FieldType;

                        if (i < names.Length - 1)
                        {
                            Instance = fieldInfo.GetValue(Instance);
                        }
                    }
                    else if (member is PropertyInfo)
                    {
                        var propertyInfo = (member as PropertyInfo);
                        type = propertyInfo.PropertyType;

                        if (i < names.Length - 1)
                        {
                            Instance = propertyInfo.GetValue(Instance, null);
                        }
                    }
                }
            }
            return member;
        }
    }
}
