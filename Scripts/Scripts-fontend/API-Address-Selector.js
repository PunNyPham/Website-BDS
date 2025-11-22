// Dropdown địa chỉ động
const citySelect = document.getElementById("city");
const districtSelect = document.getElementById("district");
const wardSelect = document.getElementById("ward");

axios
  .get(
    "https://raw.githubusercontent.com/kenzouno1/DiaGioiHanhChinhVN/master/data.json"
  )
  .then((response) => {
    const data = response.data;

    data.forEach((city) => {
      const option = document.createElement("option");
      option.value = city.Name;
      option.textContent = city.Name;
      citySelect.appendChild(option);
    });

    citySelect.addEventListener("change", function () {
      districtSelect.innerHTML =
        '<option value="">-- Chọn Quận/Huyện --</option>';
      wardSelect.innerHTML = '<option value="">-- Chọn Phường/Xã --</option>';

      const selectedCity = data.find((c) => c.Name === this.value);
      if (selectedCity) {
        selectedCity.Districts.forEach((district) => {
          const option = document.createElement("option");
          option.value = district.Name;
          option.textContent = district.Name;
          districtSelect.appendChild(option);
        });
      }
    });

    districtSelect.addEventListener("change", function () {
      wardSelect.innerHTML = '<option value="">-- Chọn Phường/Xã --</option>';

      const selectedCity = data.find((c) => c.Name === citySelect.value);
      if (selectedCity) {
        const selectedDistrict = selectedCity.Districts.find(
          (d) => d.Name === this.value
        );
        if (selectedDistrict) {
          selectedDistrict.Wards.forEach((ward) => {
            const option = document.createElement("option");
            option.value = ward.Name;
            option.textContent = ward.Name;
            wardSelect.appendChild(option);
          });
        }
      }
    });
  })
  .catch((error) => {
    console.error("Không thể tải dữ liệu địa chỉ:", error.message);
  });
