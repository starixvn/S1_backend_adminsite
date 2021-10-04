import { getUrlParameter } from "../../commJS/common";
import WeekResult from "./WeekResult";
import DayResult from "./DayResult";
import CategoryResult from "./CategoryResult";
import GameResult from "./GameResult";
import moment from "moment-timezone";
const Panel = () => {
  moment.tz.setDefault("Asia/Taipei");
  const [stateWeekDatas, setWeekDatas] = React.useState();
  const [stateDayDatas, setDayDatas] = React.useState();
  const [stateCategoryDatas, setCategoryDatas] = React.useState();
  const [stateGameDatas, setGameDatas] = React.useState();
  const [stateTpBetRecordDatas, setTpBetRecordDatas] = React.useState();
  const [stateTpBetRecordDatasShowStatus, setTpBetRecordDatasShowStatus] =
    React.useState(false);
  const [stateTpBetRecordDatasShowRawDetail, setTpBetRecordDatasShowRawDetail] =
    React.useState(false);
  const [stateGameInfo, setGameInfo] = React.useState({
    Date: null,
    GameTypeId: null,
    GameTypeName: "",
  });
  const [stateInfoType, setInfoType] = React.useState();
  let CustId = parseInt(getUrlParameter("custId"));
  const [filterData, setfilterData] = React.useState({
    StartTime: moment
      .utc(new Date())
      .add(8, "hour")
      .add(-1, "days")
      .format("l LTS"),
    EndTime: moment.utc(new Date()).add(8, "hour").format("l LTS"),
    DateQuickPicker: "0",
  });
  const [stateDatas, setDatas] = React.useState({
    FilterInfo: null,
  });

  const EditfilterData = (v, col) => {
    setfilterData({
      ...filterData,
      [col]: v,
    });
  };

  const EditDate = (v, col) => {
    let _startDate = "";
    let _endDate = "";

    switch (v) {
      case "1":
        _startDate = moment().startOf("day");
        _endDate = moment().endOf("day");
        break;
      case "2":
        _startDate = moment().subtract(1, "days");
        _endDate = moment().subtract(1, "days");

        break;
      case "3":
        _startDate = moment().startOf("week");
        _endDate = moment().endOf("week");

        break;
      case "4":
        _startDate = moment().subtract(1, "week").startOf("week");
        _endDate = moment().subtract(1, "week").endOf("week");

        break;
      case "5":
        _startDate = moment().startOf("month");
        _endDate = moment().endOf("month");

        break;
      case "6":
        _startDate = moment().subtract(1, "month").startOf("month");
        _endDate = moment().subtract(1, "month").endOf("month");

        break;
      default:
        break;
    }

    if (v !== "0") {
      setfilterData({
        ...filterData,
        ["StartTime"]: _startDate,
        ["EndTime"]: _endDate,
        ["DateQuickPicker"]: v,
      });
    } else {
      setfilterData({
        ...filterData,
        ["DateQuickPicker"]: v,
      });
    }
  };

  const ClickSubmit = () => {
    axios
      .post("/api/Payment/GetCustTpBetRecordList", {
        CustId: CustId,
        StartDate: moment(filterData.StartTime).unix(),
        EndDate: moment(filterData.EndTime).unix(),
      })
      .then((res) => {
        let result = res.data;
        if (result.errorCode == 0) {
          setWeekDatas(result.data);
          setInfoType("week");
          setDatas({
            FilterInfo: filterData,
          });
        } else {
          alert(result.message);
        }
      })
      .catch((error) => {
        alert(error);
      });
  };

  React.useEffect(() => {
    axios
      .post("/api/Payment/GetCustTpBetRecordList", {
        CustId: CustId,
        StartDate: moment(filterData.StartTime).unix(),
        EndDate: moment(filterData.EndTime).unix(),
      })
      .then(
        function (response) {
          let result = response.data;
          if (result.errorCode == 0) {
            setWeekDatas(result.data);
            setInfoType("week");
            setDatas({
              FilterInfo: filterData,
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
  }, []);

  const getDayData = (Date) => {
    axios
      .post("/api/Payment/GetCustTpBetRecordListOneDay", {
        CustId: CustId,
        selDate: moment(Date).format("YYYY-MM-DD"),
        StartDate: moment(stateDatas.FilterInfo.StartTime).unix(),
        EndDate: moment(stateDatas.FilterInfo.EndTime).unix(),
      })
      .then((res) => {
        let result = res.data;
        if (result.errorCode == 0) {
          setDayDatas(result.data);
          setInfoType("day");
        } else {
          alert(result.message);
        }
      })
      .catch((error) => {
        alert(error);
      });
  };

  const getCategoryData = (Date, CategoryId) => {
    axios
      .post("/api/Payment/GetCustTpBetRecordListbyCategoryId", {
        CustId: CustId,
        selDate: moment(Date).format("YYYY-MM-DD"),
        CategoryId: parseInt(CategoryId),
        StartDate: moment(stateDatas.FilterInfo.StartTime).unix(),
        EndDate: moment(stateDatas.FilterInfo.EndTime).unix(),
      })
      .then((res) => {
        let result = res.data;
        if (result.errorCode == 0) {
          setCategoryDatas(result.data);
          setInfoType("category");
        } else {
          alert(result.message);
        }
      })
      .catch((error) => {
        alert(error);
      });
  };

  const getGameData = (Date, GameTypeId, GameTypeName, PageNumber) => {
    axios
      .post("/api/Payment/GetCustTpBetRecordDetailListbyGameType", {
        CustId: CustId,
        selDate: moment(Date).format("YYYY-MM-DD"),
        gameTypeId: parseInt(GameTypeId),
        StartDate: moment(stateDatas.FilterInfo.StartTime).unix(),
        EndDate: moment(stateDatas.FilterInfo.EndTime).unix(),
        PageSize: 15,
        PageNumber: PageNumber,
      })
      .then((res) => {
        let result = res.data;
        if (result.errorCode == 0) {
          setGameDatas(result);
          setGameInfo({
            Date: Date,
            GameTypeId: GameTypeId,
            GameTypeName: GameTypeName,
          });
          setInfoType("game");
        } else {
          alert(result.message);
        }
      })
      .catch((error) => {
        alert(error);
      });
  };

  const getTpBetRecordDetail = (GameType, TransId, SettledTime) => {
    axios
      .post("/api/tp/GetTpBetRecordDetail", {
        CustId: CustId,
        GameType: parseInt(GameType),
        TransId: TransId,
        SettledTime: SettledTime,
        //For Test
        //GameType: parseInt(1),
        //TransId: "Test1234",
      })
      .then((res) => {
        let result = res.data;
        switch (result.errorCode) {
          case 0: //已經接廠商，取得資料成功
            break;
          case 3000: //已經接廠商，取得資料找不到
            break;
          case 3001: //尚未接廠商，取得資料找不到
            break;
          default:
            //已經接廠商，取得資料exception & 未知錯誤
            if (result.message == "") {
              alert(result.rawMessage);
            } else {
              alert(result.message);
            }
            break;
        }
        setTpBetRecordDatas(result);
        setTpBetRecordDatasShowStatus(true);
        setTpBetRecordDatasShowRawDetail(false);
      })
      .catch((error) => {
        alert(error);
      });
  };

  //Poppout Close
  const GameResult_SwitchDetailBtnClick = (flag) => {
    setTpBetRecordDatasShowRawDetail(flag);
  };

  //Poppout Close
  const GameResult_CloseBtnClick = () => {
    setTpBetRecordDatasShowStatus(false);
    setTpBetRecordDatas({});
  };

  const onClickBack = (type) => {
    setInfoType(type);
  };

  return stateInfoType === "week" ? (
    <WeekResult
      Datas={stateWeekDatas}
      ClickBack={onClickBack}
      ClickDetail={getDayData}
      FilterData={filterData}
      EditfilterData={EditfilterData}
      EditDate={EditDate}
      ClickSubmit={ClickSubmit}
    />
  ) : stateInfoType === "day" ? (
    <DayResult
      Datas={stateDayDatas}
      ClickBack={onClickBack}
      ClickDetail={getCategoryData}
    />
  ) : stateInfoType === "category" ? (
    <CategoryResult
      Datas={stateCategoryDatas}
      ClickBack={onClickBack}
      ClickDetail={getGameData}
    />
  ) : stateInfoType === "game" ? (
    <GameResult
      Datas={stateGameDatas}
      Info={stateGameInfo}
      ClickBack={onClickBack}
      ClickDetail={getGameData}
      //Popout:TpBetRecordDetail
      TpBetRecordDatas={stateTpBetRecordDatas}
      TpBetRecordDatasShowStatus={stateTpBetRecordDatasShowStatus}
      TpBetRecordDatasShowRawDetail={stateTpBetRecordDatasShowRawDetail}
      ClickTpBetRecordDetail={getTpBetRecordDetail}
      TpBetRecordCloseBtn={GameResult_CloseBtnClick}
      SwitchDetailBtn={GameResult_SwitchDetailBtnClick}
    />
  ) : null;
};

export default Panel;
