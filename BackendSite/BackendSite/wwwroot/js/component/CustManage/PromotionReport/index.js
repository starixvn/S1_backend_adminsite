import { getUrlParameter } from "../../commJS/common";
import Filter from "./Filter";
import Result from "./Result";

const Panel = () => {
  const [DateRangeDatas, setDateRange] = React.useState({
    StartTime: moment(new Date()).add(-10, "days").format("l"),
    EndTime: moment(new Date()).format("l"),
  });
  const [stateDatas, setDatas] = React.useState();
  const EditDateRangeFiled = (v, col) => {
    setDateRange({
      ...DateRangeDatas,
      [col]: v,
    });
  };

  let CustId = getUrlParameter("custId");
  let iCustId = parseInt(CustId);
  const onSubmit = () => {
    axios
      .post(
        "/api/CustManage/GetPromotionListByCustIdAndDate",
        `CustId=${CustId}&StartDate=${moment(
          new Date(DateRangeDatas.StartTime)
        ).unix()}&EndDate=${moment(
          new Date(DateRangeDatas.EndTime)
        ).unix()}&PageNumber=1&PageSize=10`
      )
      .then((res) => {
        let result = res.data;
        if (result.errorCode == 0) {
          setDatas(result.data);
        } else {
          alert(result.message);
        }
      })
      .catch((error) => {
        alert(error);
      });
  };
  const cancelBonus = (bonusId) => {
    console.log(bonusId);
    if (confirm("Are you sure Cancel Bonus ?")) {
      //let data = this.props.CustInfoData;
      axios
        .post("/api/CustManage/CancelBonusByBonusId", {
          CustId: CustId,
          BonusId: bonusId,
        })
        .then(
          function (response) {
            //Loading.UnLoading();
            let result = response.data;
            if (result.errorCode == 0) {
              this.props.onGetCustInfoDetail();
            } else {
              alert(result.message);
            }
          }.bind(this)
        )
        .catch(
          function (error) {
            //Loading.UnLoading();
            alert(error);
          }.bind(this)
        );
    }
  };
  return (
    <>
      <Filter
        ClickSubmit={onSubmit}
        DateRangeDatas={DateRangeDatas}
        EditDateRangeFiled={EditDateRangeFiled}
      />
      {stateDatas === undefined ? null : <Result Datas={stateDatas} CancelBonus={cancelBonus}/>}
    </>
  );
};

export default Panel;
