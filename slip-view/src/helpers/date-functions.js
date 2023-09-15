export const formatDate = (date) => {
  var dt = new Date(date);
  let hours = dt.getHours();
  let minutes = dt.getMinutes();
  let month = dt.getMonth() + 1;
  let day = dt.getDate();
  day = day.toString().padStart(2, "0");
  month = month.toString().padStart(2, "0");
  hours = hours.toString().padStart(2, "0");
  minutes = minutes.toString().padStart(2, "0");
  return (date = day + "." + month + " " + hours + ":" + minutes);
};
export const formatDateWithDay = (date) => {
  const dt = new Date(date);
  const hours = dt.getHours().toString().padStart(2, "0");
  const minutes = dt.getMinutes().toString().padStart(2, "0");
  const month = (dt.getMonth() + 1).toString().padStart(2, "0");
  const day = dt.getDate().toString().padStart(2, "0");
  const options = {
    weekday: "short",
    year: "numeric",
    month: "long",
    day: "numeric",
  };
  const locale = "sr-Latn-RS";
  const formattedDate = dt.toLocaleDateString(locale, options);
  const dayOfWeek = formattedDate.substring(0, 3);
  return `${dayOfWeek.charAt(0).toUpperCase()}${dayOfWeek.slice(
    1
  )}, ${day}.${month} ${hours}:${minutes}`;
};
