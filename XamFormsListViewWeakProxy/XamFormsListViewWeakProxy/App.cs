using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Xamarin.Forms;

namespace XamFormsListViewWeakProxy
{
	public class App : Application
	{
        MyList theList = new MyList();

		public App ()
		{
			// The root page of your application
			MainPage = new ContentPage {
				Content = new StackLayout {
					VerticalOptions = LayoutOptions.Center,
					Children = {
						new ListView(){
                            ItemsSource = theList
                        }
					}
				}
			};


            Timer stateTimer = new Timer(OnClick, null, 2000, 2000);
            theList.CollectionChanged += theList_CollectionChanged;
		}

        void theList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.NewItems != null)
                foreach(Monkey mono in e.NewItems)
                {
                    Debug.Write(mono);
                }
            
        }

        private void OnClick(object state)
        {
            theList.AddMonkey();
        }

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}

    public class Monkey
    {
        static int i = 0;
        public Monkey()
        {
            Name = string.Format("Monkey: {0}", i);
            i++;
        }

        public override string ToString()
        {
            return Name;
        }

        public string Name { get;set; }
    }

    public class MyList : List<Monkey>, INotifyCollectionChanged
    {
        List<WeakHandler> handlers = new List<WeakHandler>();

        public MyList()
        {

        }
        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add { handlers.Add(new WeakHandler(this, value)); }
            remove { throw new NotImplementedException(); }
        }


        public void AddMonkey()
        {
            var me = new Monkey();
            this.Add(me);

            foreach (var handler in handlers.ToList())
            {
                if (handler.IsActive)
                {
                    handler.Handler.DynamicInvoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, me));
                }
                else
                {
                    handlers.Remove(handler);
                    Debug.WriteLine("Handler Removed");
                }
            }

            if(this.Count % 10 == 0)
            {
                //Force garbage collection to illustrate point
                GC.Collect();
            }
        }
    }


    internal class WeakHandler
    {
        WeakReference source;
        WeakReference originalHandler;

        public bool IsActive
        {
            get { return this.source != null && this.source.IsAlive && this.originalHandler != null && this.originalHandler.IsAlive; }
        }

        public NotifyCollectionChangedEventHandler Handler
        {
            get
            {
                if (this.originalHandler == null)
                {
                    return default(NotifyCollectionChangedEventHandler);
                }
                else
                {
                    return (NotifyCollectionChangedEventHandler)this.originalHandler.Target;
                }
            }
        }

        public WeakHandler(object source, NotifyCollectionChangedEventHandler originalHandler)
        {
            this.source = new WeakReference(source);
            this.originalHandler = new WeakReference(originalHandler);
        }
    }

}
