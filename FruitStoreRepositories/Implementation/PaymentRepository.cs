using Dapper;
using FruitStoreModels.Order;
using FruitStoreModels.Payment;
using FruitStoreRepositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Razorpay.Api;
using System.Data;
using System.Data.SqlClient;

namespace FruitStoreRepositories.Implementation
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly IConfiguration _configuration;
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<PaymentRepository> _logger;
        private readonly RazorpayClient _razorpayClient;

        public PaymentRepository(IConfiguration configuration, ILogger<PaymentRepository> logger, RazorpayClient razorpayClient, IOrderRepository orderRepository)
        {
            _configuration = configuration;
            _logger = logger;
            _razorpayClient = razorpayClient;
            _orderRepository = orderRepository;
        }

        //... Method for cofirm & pay for order at razorpay ...//
        public async Task<int> CompleteOrderProcessAsync(CompleteOrderProcess completeOrderProcess)
        {
            try
            {
                _logger.LogInformation($"Starting to complete order process and payment at razorpay order in method CompleteOrderProcessAsync for UserId: {completeOrderProcess.UserId} \n");

                int updatedStatus = 0;

                if (ConfirmPaymentAsync(completeOrderProcess.RazorpayOrderId, completeOrderProcess.PaymentId, completeOrderProcess.Signature))
                {
                    _logger.LogInformation($"Payment confirmation successful. Proceeding to update payment status in the database in method CompleteOrderProcessAsync for UserId: {completeOrderProcess.UserId} \n");

                    UpdatePaymentStatus updatePaymentStatus = new UpdatePaymentStatus()
                    {
                        OrderId = completeOrderProcess.OrderId,
                        UserId = completeOrderProcess.UserId,
                        ModeOfPayment = completeOrderProcess.ModeOfPayment,
                        OrderStatus = "Placed"
                    };

                    updatedStatus = await UpdatePaymentDetailsAsync(updatePaymentStatus);

                    if (updatedStatus != 0)
                    {
                        _logger.LogInformation($"Payment status and order process updated successfully in the database for method CompleteOrderProcessAsync for UserId: {completeOrderProcess.UserId} \n");
                    }
                    else
                    {
                        _logger.LogError($"Failed to update payment status and order process in the database for method method CompleteOrderProcessAsync for UserId: {completeOrderProcess.UserId} \n");
                    }

                }
                return updatedStatus;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while completing the update payment status and order process in method CompleteOrderProcessAsync for UserId: {completeOrderProcess.UserId}: {ex.Message} \n");
                throw;
            }

        }

        //... Method for create order at razorpay ...//
        public async Task<OrderDetails> ProcessOrderAsync(AddOrderDetails orderRequest)
        {
            try
            {
                int currentOrderStatusId = 0;

                int updatedStatus = 0;

                Random randomObj = new Random();

                string transactionId = randomObj.Next(10000000, 100000000).ToString();

                _logger.LogInformation($"Starting to process razorpay order in method ProcessOrderAsync for UserId : {orderRequest.UserId} \n");

                _logger.LogInformation($"Generating transaction Id in method ProcessOrderAsync for UserId : {orderRequest.UserId} \n");

                // Create a DataTable
                DataTable dataTable = new DataTable();
                dataTable.Columns.Add("ProductImage", typeof(string));
                dataTable.Columns.Add("ProductName", typeof(string));
                dataTable.Columns.Add("Price", typeof(decimal));
                dataTable.Columns.Add("Quantity", typeof(int));

                // Populate DataTable with objects data
                foreach (OrderCartDetails obj in orderRequest.cartDetails)
                {
                    // Create a new row
                    DataRow row = dataTable.NewRow();

                    // Set values of each column in the row
                    row["ProductImage"] = obj.ProductImage;
                    row["ProductName"] = obj.ProductName;
                    row["Price"] = obj.Price;
                    row["Quantity"] = obj.Quantity;

                    // Add the row to the DataTable
                    dataTable.Rows.Add(row);
                }
                if (orderRequest.ModeOfPayment == "Cash-On-Delivery")
                {
                    InsertOrderDetails orderDetails = new InsertOrderDetails()
                    {
                        CartDetails = dataTable,
                        SubTotal = orderRequest.SubTotal,
                        Charges = orderRequest.Charges,
                        Total = orderRequest.Total,
                        OrderStatus = "Created",
                        Receipt = transactionId,
                        UserId = orderRequest.UserId,
                        ModeOfPayment = orderRequest.ModeOfPayment
                    };

                    currentOrderStatusId = await _orderRepository.InsertOrderDetailsAsync(orderDetails);

                    if (currentOrderStatusId != 0)
                    {
                        UpdatePaymentStatus updatePaymentStatus = new UpdatePaymentStatus()
                        {
                            OrderId = currentOrderStatusId,
                            UserId = orderRequest.UserId,
                            ModeOfPayment = orderRequest.ModeOfPayment,
                            OrderStatus = "Created"
                        };
                        updatedStatus = await UpdatePaymentDetailsAsync(updatePaymentStatus);
                        if (updatedStatus == 0)
                        {
                            currentOrderStatusId = 0;
                        }
                    }
                }
                else
                {

                    _logger.LogInformation($"Generation of transaction Id in method ProcessOrderAsync completed for UserId : {orderRequest.UserId} \n");

                    _logger.LogInformation($"Creating razorpay order details object in method ProcessOrderAsync for UserId : {orderRequest.UserId} \n");

                    Razorpay.Api.RazorpayClient client = new Razorpay.Api.RazorpayClient(_configuration["RazorPay:Key"], _configuration["RazorPay:Secret"]);
                    Dictionary<string, object> options = new Dictionary<string, object>();
                    options.Add("amount", orderRequest.Total * 100);
                    options.Add("receipt", transactionId);
                    options.Add("currency", "INR");
                    options.Add("payment_capture", "0");

                    _logger.LogInformation($"Creating razorpay order details object in method ProcessOrderAsync completed for UserId : {orderRequest.UserId} \n");

                    Razorpay.Api.Order orderResponse = await Task.Run(() => client.Order.Create(options));

                    string orderId = orderResponse["id"].ToString();

                    InsertOrderDetails orderDetails = new InsertOrderDetails()
                    {
                        CartDetails = dataTable,
                        SubTotal = orderRequest.SubTotal,
                        Charges = orderRequest.Charges,
                        Total = orderRequest.Total,
                        AmountPaid = orderResponse["amount_paid"],
                        AmountDue = orderResponse["amount_due"],
                        Currency = orderResponse["currency"],
                        Receipt = orderResponse["receipt"],
                        OrderStatus = "Created",
                        UserId = orderRequest.UserId,
                        RazorPayOrderId = orderId,
                        ModeOfPayment = orderRequest.ModeOfPayment
                    };

                    currentOrderStatusId = await _orderRepository.InsertOrderDetailsAsync(orderDetails);
                }

                if (currentOrderStatusId != 0)
                {
                    var createdOrderDetails = await _orderRepository.GetOrderDetailsByOrderIdAsync(currentOrderStatusId);

                    if (createdOrderDetails != null)
                    {
                        _logger.LogInformation($"Fetching Order details successfully from DB in method ProcessOrderAsync for OrderId : {currentOrderStatusId} \n");
                        return createdOrderDetails;
                    }
                }
                _logger.LogError($"Processing razorpay order in method ProcessOrderAsync failed for UserId : {orderRequest.UserId} \n");
                return null;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while process razorpay order in method ProcessOrderAsync for UserId : {orderRequest.UserId} : {ex.Message} \n");
                throw;
            }
        }

        //... Method for update payment at razorpay ...//
        public async Task<int> UpdatePaymentDetailsAsync(UpdatePaymentStatus paymentStatus)
        {
            try
            {
                _logger.LogInformation($"Updating payment status starts in method UpdatePaymentDetailsAsync for user Id : {paymentStatus.UserId} \n");

                int updatedStatusId = 0;

                using (SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    _logger.LogInformation("Attempting to open database connection \n");

                    await dbConnection.OpenAsync();

                    _logger.LogInformation("Database connection opened successfully \n");

                    var parameters = new DynamicParameters();
                    parameters.Add("@OrderId", paymentStatus.OrderId);
                    parameters.Add("@UserId", paymentStatus.UserId);
                    parameters.Add("@ModeOfPayment", paymentStatus.ModeOfPayment);
                    parameters.Add("@OrderStatus", paymentStatus.OrderStatus);
                    parameters.Add("@UpdatedStatusId", dbType: DbType.Int32, direction: ParameterDirection.Output);

                    _logger.LogInformation("Executing stored procedure stp_UpdatePaymentStatus  \n");

                    await dbConnection.ExecuteScalarAsync<int>(
                       "stp_UpdatePaymentStatus",
                       parameters,
                       commandType: CommandType.StoredProcedure
                   );

                    _logger.LogInformation("Stored procedure execution completed successfully \n");

                    updatedStatusId = parameters.Get<int>("@UpdatedStatusId");

                    if (updatedStatusId != 0)
                    {
                        _logger.LogInformation($"Payment status updated successfully in the database for method UpdatePaymentDetailsAsync for UserId: {paymentStatus.UserId} \n");
                    }
                    else
                    {
                        _logger.LogError($"Failed to update payment status in the database for method UpdatePaymentDetailsAsync for UserId: {paymentStatus.UserId} \n");
                    }
                }
                return updatedStatusId;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while update payment status in method UpdatePaymentDetailsAsync for UserId: {paymentStatus.UserId} : {ex.Message} \n");
                throw;
            }
        }

        //... Method for verify payment confirmation with signature at razorpay ...//
        private bool ConfirmPaymentAsync(string orderId, string paymentId, string signature)
        {
            _logger.LogInformation($"Confirming payment starts in method ConfirmPayment for orderId : {orderId} \n");
            try
            {
                Dictionary<string, string> attributes = new Dictionary<string, string>();

                attributes.Add("razorpay_payment_id", paymentId);
                attributes.Add("razorpay_order_id", orderId);
                attributes.Add("razorpay_signature", signature);
                Utils.verifyPaymentSignature(attributes);
                _logger.LogInformation($"Confirming payment completed in method ConfirmPayment for orderId : {orderId} \n");
                return true;

            }
            catch (Exception)
            { throw; }
        }
    }
}
