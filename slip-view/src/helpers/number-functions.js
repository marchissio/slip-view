export const convertToCurrency = (value, zeros = true) => {
	if (value === "None") return 0;
	if (!value) return 0;
	if (value == "/") {
		return value;
	}
	const formatter = new Intl.NumberFormat("en-US", {
		style: "currency",
		currency: "USD",
		minimumFractionDigits: 2,
	});
	if (zeros) {
		return formatter.format(value).replace("$", "");
	} else {
		return formatter
			.format(value)
			.replace("$", "")
			.substring(0, formatter.format(value).replace("$", "").length - 3);
	}
}