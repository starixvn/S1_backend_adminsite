import { getUrlParameter } from "../../commJS/common";
import Filter from "./Filter";
import Result from "./Result";

const Panel = () => {
  const [DateRangeDatas, setDateRange] = React.useState({
    StartTime: moment(new Date()).add(-10, "days").format("l"),
    EndTime: moment(new Date()).format("l"),
  });
  //const [stateDatas, setDatas] = React.useState();
  const [stateDatas, setDatas] = React.useState({
    PageInfo: null,
    Datas: null,
    FilterInfo: null,
  });
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
        ).add(1, 'days').unix() -1}&PageNumber=1&PageSize=10`
      )
      .then((res) => {
        let result = res.data;
        if (result.errorCode == 0) {
          //setDatas(result.data);
          setDatas({
            PageInfo: {
              totalPages: res.data.totalPages,
              pageNumber: 1,
              rowCount: res.data.rowCount,
            },
            Datas: [...res.data.data],
            FilterInfo: {
              ...DateRangeDatas,
            },
          });
        } else {
          alert(result.message);
        }
      })
      .catch((error) => {
        alert(error);
      });
  };
  const Result_ChangePage = (PageNumber) => {
    axios
      .post(
        "/api/CustManage/GetPromotionListByCustIdAndDate",
        `CustId=${CustId}&StartDate=${moment(
          new Date(stateDatas.FilterInfo.StartTime)
        ).unix()}&EndDate=${moment(
          new Date(stateDatas.FilterInfo.EndTime)
        ).add(1, 'days').unix() -1}&PageNumber=${PageNumber}&PageSize=10`
      )
      .then(
        function (response) {
          let result = response.data;
          if (result.errorCode == 0) {
            setDatas({
              ...stateDatas,
              PageInfo: {
                totalPages: response.data.totalPages,
                pageNumber: PageNumber,
                rowCount: response.data.rowCount,
              },
              Datas: [...response.data.data],
            });
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
  };
  const cancelBonus = (bonusId) => {
    if (confirm("Are you sure Cancel Bonus ?")) {
      //let data = this.props.CustInfoData;
      axios
        .post("/api/CustManage/CancelBonusByBonusId", {
          CustId: iCustId,
          BonusId: bonusId,
        })
        .then(
          function (response) {
            //Loading.UnLoading();
            let result = response.data;
            if (result.errorCode == 0) {
              onSubmit();
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
      {stateDatas.Datas === null ? null : (
        <Result
          Datas={stateDatas.Datas}
          CancelBonus={cancelBonus}
          ChangePage={Result_ChangePage}
          PageInfo={stateDatas.PageInfo}
        />
      )}
    </>
  );
};

export default Panel;
