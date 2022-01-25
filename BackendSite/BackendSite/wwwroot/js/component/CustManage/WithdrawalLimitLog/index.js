import Filter from "./Filter";
import { getUrlParameter } from "../../commJS/common";
import Result from "./Result";
import moment from "moment-timezone";

const Panel = () => {
  const [DateRangeDatas, setDateRange] = React.useState({
    StartTime: moment(new Date()).add(-10, "days").format("l"),
    EndTime: moment(new Date()).format("l"),
  });
  const [stateDatas, setDatas] = React.useState({
    PageInfo: null,
    Datas: null,
    FilterInfo: null,
  });

  let CustId = getUrlParameter("custId");
  let iCustId = parseInt(CustId);
  const EditDateRangeFiled = (v, col) => {
    setDateRange({
      ...DateRangeDatas,
      [col]: v,
    });
  };

  const onSubmit = () => {
    axios
      .post("/api/CustManage/GetWithdrawalLimitLog", {
        CustId: iCustId,
        StartDate: moment(new Date(DateRangeDatas.StartTime)).unix(),
        EndDate:
          moment(new Date(DateRangeDatas.EndTime)).add(1, "days").unix() - 1,
        PageNumber: parseInt(1),
        PageSize: parseInt(10),
      })
      .then(
        function (response) {
          //Loading.UnLoading();
          let result = response.data;
          if (result.errorCode == 0) {
            setDatas({
              PageInfo: {
                totalPages: result.totalPages,
                pageNumber: 1,
                rowCount: result.rowCount,
              },
              Datas: [...result.data],
              FilterInfo: {
                ...DateRangeDatas,
              },
            });
          } else {
            alert(result.message);
          }
        }.bind(this)
      )
      .catch(
        function (error) {
          alert(error);
        }.bind(this)
      );
  };
  const Result_ChangePage = (PageNumber) => {
    axios
      .post("/api/CustManage/GetWithdrawalLimitLog", {
        CustId: iCustId,
        StartDate: moment(new Date(DateRangeDatas.StartTime)).unix(),
        EndDate:
          moment(new Date(DateRangeDatas.EndTime)).add(1, "days").unix() - 1,
        PageNumber: parseInt(PageNumber),
        PageSize: parseInt(10),
      })
      .then(
        function (response) {
          //Loading.UnLoading();
          let result = response.data;
          if (result.errorCode == 0) {
            setDatas({
              ...stateDatas,
              PageInfo: {
                totalPages: result.totalPages,
                pageNumber: PageNumber,
                rowCount: result.rowCount,
              },
              Datas: [...result.data],
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
          ChangePage={Result_ChangePage}
          PageInfo={stateDatas.PageInfo}
        />
      )}
    </>
  );
};

export default Panel;
