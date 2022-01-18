import Filter from "./Filter";

const Panel = () => {
  const [DateRangeDatas, setDateRange] = React.useState({
    StartTime: moment(new Date()).add(-10, "days").format("l"),
    EndTime: moment(new Date()).format("l"),
  });
  let CustId = getUrlParameter("custId");
  let iCustId = parseInt(CustId);
  const EditDateRangeFiled = (v, col) => {
    setDateRange({
      ...DateRangeDatas,
      [col]: v,
    });
  };
  const onSubmit = () => {}
  return (
    <>
      <Filter
        ClickSubmit={onSubmit}
        DateRangeDatas={DateRangeDatas}
        EditDateRangeFiled={EditDateRangeFiled}
      />
    </>
  );
};

export default Panel;
