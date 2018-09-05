﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;


namespace SR
{
    public interface ASignatures
    {

        string Connect();
        string DisConnect();
        string SendMessageWC(string message);
        string SendMessage(string message);
        string ReceiveMessageWC(string queuename);
        string ReceiveMessage(string queuename);
        string MessagesInQueue(string queuename);
        string Ack(string queuename);
        string Nack(string queuename);

    }

    [Guid("AB634001-F13D-11d0-A459-004095E1DAEA")]// стандартный GUID для IInitDone ссылка http://soaron.fromru.com/vkhints.htm
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IInitDone
    {
        /// <summary>
        /// Инициализация компонента
        /// </summary>
        /// <param name="connection">reference to IDispatch</param>
        void Init([MarshalAs(UnmanagedType.IDispatch)] object connection);

        /// <summary>
        /// Вызывается перед уничтожением компонента
        /// </summary>
        void Done();

        /// <summary>
        /// Возвращается инициализационная информация
        /// </summary>
        /// <param name="info">Component information</param>
        void GetInfo([MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_VARIANT)] ref object[] info);
    }

    [ClassInterface(ClassInterfaceType.AutoDual)]
    
    //public class ArsClass : ASignatures, IInitDone
    public class OneSRabbit : ASignatures, IInitDone
    {

        /// <summary>
        /// Инициализация компонента
        /// </summary>
        /// <param name="connection">reference to IDispatch</param>
        public void Init([MarshalAs(UnmanagedType.IDispatch)] object connection)
        {
            //asyncEvent = (IAsyncEvent)connection;
            //statusLine = (IStatusLine)connection;
        }

        /// <summary>
        /// Возвращается информация о компоненте
        /// </summary>
        /// <param name="info">Component information</param>
        public void GetInfo([MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_VARIANT)] ref object[] info)
        {
            info[0] = 2000;
        }

        public void Done()
        {
        }

        const bool autoAck = false;
        public string UserName { get; set; }
        public string Password { get; set; }
        public string HostName { get; set; }
        public int Port { get; set; }

        public string Exchange { get; set; }
        public string RoutingKey { get; set; }

        private ConnectionFactory fact = null;
        private IConnection conn = null;
        private IModel chan = null;
        

        //CONNECT
        public string Connect()
        {
            if (fact == null)
            {
                try
                {
                    fact = new ConnectionFactory()
                      {
                          HostName = HostName,
                          UserName = UserName,
                          Password = Password,
                          Port = Port
                      };
                    
                    conn = fact.CreateConnection();

                    chan = conn.CreateModel();

                    return "Connection established!";
                }
                catch (Exception e)
                {
                    return e.ToString();
                }
            }
            else 
            {
                return "Connection already established!";
            }
        }

        //DISCONNECT
        public string DisConnect()
        {
            if (fact != null)
            {
                chan = null;
                conn = null;
                fact = null;
                return "Connection closed!";
            }
            else
            {
                return "No active connection!";
            }
        }


        //SENDING
        public string SendMessageWC(string message)
        {
            if (fact != null)
            {
                try
                {

                   var body = Encoding.UTF8.GetBytes(message);
                   chan.BasicPublish(Exchange, RoutingKey, null, body);
                   
                }
                catch (Exception e)
                {
                    return e.ToString();
                }

                return "OK!";
            }
            else
            {
                return "No active connection!";
            }
        
        }

        public string SendMessage(string message) 
        {

            try 
            {
                ConnectionFactory factory = new ConnectionFactory()
                {
                    HostName = HostName,
                    UserName = UserName,
                    Password = Password,
                    Port = Port
                };

                using (var connection = factory.CreateConnection()) 
                {
                    using (var chanell = connection.CreateModel())
                    {
                        var body = Encoding.UTF8.GetBytes(message);
                        chanell.BasicPublish(Exchange, RoutingKey, null, body);
                    
                    }
                }
            }
            catch(Exception e)
            {
                return e.ToString();                
            }

            return "OK!";
        }

        //RECEIVING
        public string ReceiveMessageWC(string queuename)
        {
            try
            {
                    chan.QueueDeclare(queue: queuename,
                                      durable: false,
                                      exclusive: false,
                                      autoDelete: false,
                                      arguments: null);

                    var consumer = new QueueingBasicConsumer(chan);

                    chan.BasicConsume(queuename, autoAck, consumer);

                    var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();
                    byte[] body = ea.Body;
                    string message = System.Text.Encoding.UTF8.GetString(body);

                    return message;
              }
            catch (Exception e)
            {
                return e.ToString();
            }
        } //ReceiveMessageWC

        public string ReceiveMessage(string queuename)
        {
            try
            {
                ConnectionFactory factory = new ConnectionFactory()
                {
                    UserName = UserName,
                    Password = Password,
                    HostName = HostName,
                    Port = Port
                };

                using (var connection = factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        channel.QueueDeclare(queue: queuename,
                                             durable: false,
                                             exclusive: false,
                                             autoDelete: false,
                                             arguments: null);

                        var consumer = new QueueingBasicConsumer(channel);
                        
                        
                        const bool autoAck = false;
                        channel.BasicConsume(queuename, autoAck, consumer);

                        var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();
                        byte[] body = ea.Body;
                        string message = System.Text.Encoding.UTF8.GetString(body);

                        return message;
                     }
                }
            }
            catch(Exception e)
            {
                return e.ToString();
            }
        }//ReceiveMessage


        public string AckWC(string queuename)
        {
            try
            {
                var connection = this.conn;
                var chanell = this.chan;
                using (connection)
                {
                    using (chanell)
                    {

                        chanell.QueueDeclare(queue: queuename,
                                          durable: false,
                                          exclusive: false,
                                          autoDelete: false,
                                          arguments: null);

                        var consumer = new QueueingBasicConsumer(chanell);

                        chanell.BasicConsume(queuename, autoAck, consumer);
                        var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();

                        chanell.BasicAck(ea.DeliveryTag, false);
                        return "OK!";
                    }
                }
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }//AckWS

        //
        public string Ack(string queuename)
        {
            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = HostName,
                    UserName = UserName,
                    Password = Password,
                    Port = Port
                };

                using (var connection = factory.CreateConnection())
                {
                    using (var chanell = connection.CreateModel())
                    {

                        chanell.QueueDeclare(queue: queuename,
                                          durable: false,
                                          exclusive: false,
                                          autoDelete: false,
                                          arguments: null);

                        //var consumer = new EventingBasicConsumer(channel);
                        var consumer = new QueueingBasicConsumer(chanell);

                        chanell.BasicConsume(queuename, autoAck, consumer);
                        var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();

                        chanell.BasicAck(ea.DeliveryTag, false);
                        return "OK!";
                    }
                }
            }
            catch (Exception e)
            {
                return e.ToString();
            }     
        }//Ack


        public string Nack(string queuename)
        {
            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = HostName,
                    UserName = UserName,
                    Password = Password,
                    Port = Port
                };

                using (var connection = factory.CreateConnection())
                {
                    using (var chanell = connection.CreateModel())
                    {
                        chanell.QueueDeclare(queue: queuename,
                                          durable: false,
                                          exclusive: false,
                                          autoDelete: false,
                                          arguments: null);

                        //var consumer = new EventingBasicConsumer(channel);
                        var consumer = new QueueingBasicConsumer(chanell);

                        chanell.BasicConsume(queuename, autoAck, consumer);
                        var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();

                        chanell.BasicNack(ea.DeliveryTag, false, false);
                        return "OK!";
                    }
                }
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }//Nack


        public string MessagesInQueueWC(string queuename)
        {
            string kol = "";
            try
            {
                var connection = this.conn;
                var chanell = this.chan;
                using (connection)
                {
                    using (chanell)
                    {

                        var response = chanell.QueueDeclarePassive(queuename);
                        kol = response.MessageCount.ToString();
                        response = null;
                    }
                }

            }
            catch (Exception e)
            {
                kol = null;
                return e.ToString();
            }

            return kol;
        }

        public string MessagesInQueue(string queuename)
        {
            string kol = "";
            try
            {
                ConnectionFactory factory = new ConnectionFactory()
                {
                    UserName = UserName,
                    Password = Password,
                    HostName = HostName,
                    Port = Port
                };

                using (var connection = factory.CreateConnection())
                {
                    using (var chanell = connection.CreateModel())
                    {

                        var response = chanell.QueueDeclarePassive(queuename);
                        kol = response.MessageCount.ToString();
                    }
                }

            }
            catch (Exception e)
            {
                kol = null;
                return e.ToString();
            }

            return kol;
        }

  
    }
}