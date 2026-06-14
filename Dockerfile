# Sử dụng base image từ Microsoft để build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy file .csproj từ thư mục con vào container
COPY ["DoAn_HotelBooking/DoAn_HotelBooking.csproj", "DoAn_HotelBooking/"]

# Restore các thư viện
RUN dotnet restore "DoAn_HotelBooking/DoAn_HotelBooking.csproj"

# Copy toàn bộ source code
COPY . .

# Build ứng dụng với cấu hình Release
RUN dotnet publish "DoAn_HotelBooking/DoAn_HotelBooking.csproj" -c Release -o /app/publish

# Tạo image chạy ứng dụng
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# BỔ SUNG: Mở cổng 8080 theo chuẩn mặc định của .NET 8
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "DoAn_HotelBooking.dll"]