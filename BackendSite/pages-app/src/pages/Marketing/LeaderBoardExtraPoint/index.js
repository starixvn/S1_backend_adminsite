import React, { useState, useRef, useEffect } from "react";
import ReactDOM from "react-dom";
import Filter from "./Filter";
import Result from "./Result";
import WebAPI from "../../../module/WebAPI";
import qs from "qs";
import moment from "moment-timezone";
import { api } from "../../../module/WebAPI";
import EditPanel from "./Edit";

const App = () => {
  moment.tz.setDefault("Asia/Taipei");
  const [stateFilter, setFilter] = useState({
    SelectSite: [],
    StartDate: moment().add(-10, "days").format("l"),
    EndDate: moment().format("l"),
    Currency: "0",
    User: "",
    Category: "0",
    BaseCurrency: 0,
    DateQuickPicker:"0"
  });
  const [stateExtraPointitem, setExtraPointitem] = useState({
    ActiveDate:moment().format("l"),
    CategoryId:1,
    UserName:"",
    ExtraPoint:0
  });
  const [stateDatas, setDatas] = useState({
    PageInfo: null,
    Datas: null,
    FilterInfo: null,
    IsAdd: false,
    SettingDatas: null,
  });

  const [siteList, setSiteList] = useState();
  const [CategoryList, setCategoryList] = useState(null);

  

  useEffect(() => {
    api.post("/api/Payment/GetTPCategoryListByVipRebate").then((res) => {
      if (res.data.errorCode === 0) {
        setCategoryList(res.data.data);
      } else {
        alert(res.data.message ?? "failed to get data.");
      }
    });
  }, []);
  useEffect(() => {
    let param = {
      Url: "Common/AdminUserSiteList",
    };
    WebAPI.PostData(param).then((res) => {
      if (res.data.errorCode === 0) {
        setSiteList(res.data.data);
        let initSiteNameFilter = GetInitSelectedSiteName(res.data.data);
        
          const SiteIds = [];
					initSiteNameFilter.map((o) => {
						SiteIds.push(o.value);
					});
          let siteListOptions = "";
          if (res.data.data) {
            siteListOptions = res.data.data.map((i) => {
              return {
                value: Number(i.siteId),
                displayText: i.siteName,
              };
            });
          }
          Filter_CheckBox_CheckAll("All", "SelectSite",siteListOptions);
          //DatePicker_OnChange(3);
          console.log("stateFilter",stateFilter);
          //Filter_Submit();
          let Data = {
            SelectSite: SiteIds,
            Currency: 0,
            CategoryId: 0,
            //StartDate: moment.utc(moment().add(-10, "days").format("l")).unix(),
            StartDate: moment.utc(moment().add(-10, "days").format("l")).unix(),
            EndDate: moment.utc(moment().format("l")).unix(),
            PageNumber: 1,
            PageSize: 10,
          };
          console.log('Data',Data);
          api
            .post("/api/BonusManage/GetLeaderboardExtraPointList", Data)
            .then(function (res) {
              if (res.data.errorCode === 0) {
                setDatas({
                  PageInfo: {
                    totalPages: res.data.totalPages,
                    pageNumber: 1,
                    rowCount: res.data.rowCount,
                    currencyAmount: res.data.currencyAmount
                  },
                  Datas: [...res.data.data],
                  FilterInfo: {
                    ...stateFilter,
                  },
                });
              } else {
                alert(res.data.message ?? "failed to get data.");
              }
            })
      .catch(function (error) {
        alert(error);
      });

      } else {
        alert(res.data.message ?? "failed to get data.");
      }
    });
  }, []);


  // useEffect(() => {
  //   //Filter_CheckBox_CheckAll
  //   console.log(stateDatas.FilterInfo);
  //   let Data = {
  //     SelectSite: [1,2,3],
  //     Currency: 0,
  //     CategoryId: 0,
  //     StartDate: moment().startOf("week").unix(),
  //     EndDate: moment().endOf("week").unix(),
  //     PageNumber: 1,
  //     PageSize: 10,
  //   };

  //   api
  //     .post("/api/Payment/GetVipRebateInfoList", Data)
  //     .then(function (res) {
  //       if (res.data.errorCode === 0) {
  //         setDatas({
  //           ...stateDatas,
  //           PageInfo: {
  //             ...stateDatas.PageInfo,
  //             pageNumber: 1,
  //           },
  //           Datas: [...res.data.data],
  //         });
  //       } else {
  //         alert(res.data.message ?? "failed to get data.");
  //       }
  //     })
  //     .catch(function (error) {
  //       alert(error);
  //     });
  //   }, []);

    const GetInitSelectedSiteName = (props) => {
      let headerRows = [];
      props.map(x => {
        headerRows.push({
          label: x.siteName,
          value: x.siteId,
        })
      })
      return headerRows;
    };
  const DatePicker_OnChange = (props) => {
    let _startDate = "";
    let _endDate = "";

    switch (props) {
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
    if (props !== "0") {
      setFilter({
        ...stateFilter,
        ["StartDate"]: _startDate,
        ["EndDate"]: _endDate,
        ["DateQuickPicker"]:props
      });
    }else{
      setFilter({
        ...stateFilter,
        ["DateQuickPicker"]:props
      });
    }
  };

  const Filter_OnChange = (v, dataColumn) => {
    if (dataColumn === "StartDate" || dataColumn === "EndDate") {
      setFilter({
        ...stateFilter,
        [dataColumn]: v,
        ["DateQuickPicker"]:"0"
      });
    } else {
      setFilter({
        ...stateFilter,
        [dataColumn]: v,
      });
    }
  };

  const addExtraPoint_OnChange = (v, dataColumn) => {
    console.log("v:",v,"dataColumn",dataColumn,stateExtraPointitem);
    setExtraPointitem({
      ...stateExtraPointitem,
      [dataColumn]: v,
    });

  };

  const Filter_CheckBox_OnChange = (v, dataColumn) => {
    let isRemove = stateFilter[dataColumn].includes(v);
    let newChecked = isRemove
      ? stateFilter[dataColumn].filter((value) => value !== v)
      : stateFilter[dataColumn].push(v);
    let currentTab = stateFilter[dataColumn];
    if (newChecked.length === 0) {
      currentTab = [];
    } else if (currentTab === null) {
      currentTab = newChecked;
    }

    if (isRemove && !newChecked.includes(currentTab)) {
      currentTab = newChecked.length === 0 ? [] : newChecked;
    }
    setFilter({
      ...stateFilter,
      [dataColumn]: currentTab,
    });
  };
  const ClickAdd = () => {
    setDatas({
      ...stateDatas,
      IsAdd: true,
    });
    console.log(stateDatas);
  };
  const Filter_CheckBox_CheckAll = (v, dataColumn, options) => {
    console.log(v,dataColumn,options);
    if (v !== "All") return false;
    console.log("stateFilter[dataColumn].length",stateFilter[dataColumn].length);
    console.log("options.length",options.length);
    let currentTab =
      stateFilter[dataColumn].length === options.length
        ? []
        : options.map((o) => {
            return o.value;
          });
          console.log("options.currentTab",currentTab);
    setFilter({
      ...stateFilter,
      [dataColumn]: currentTab,
    });
  };
  const Filter_Submit = () => {
    console.log('stateFilter',stateFilter);
    if (stateFilter.SelectSite.length === 0) {
      alert("Please Choose at least one Site");
      return false;
    }
   
    let Data = {
      SelectSite: stateFilter.SelectSite,
      Currency: parseInt(stateFilter.Currency),
      CategoryId: parseInt(stateFilter.Category),
      StartDate: moment.utc(stateFilter.StartDate).unix(),
      EndDate: moment.utc(stateFilter.EndDate).unix(),
      PageNumber: 1,
      PageSize: 10
    };
    console.log('Data',Data);
    api
      .post("/api/BonusManage/GetLeaderboardExtraPointList", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          setDatas({
            PageInfo: {
              totalPages: res.data.totalPages,
              pageNumber: 1,
              rowCount: res.data.rowCount,
              currencyAmount: res.data.currencyAmount
            },
            Datas: [...res.data.data],
            FilterInfo: {
              ...stateFilter,
            },
          });
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };

  const Result_ChangePage = (PageNumber) => {
    if (stateFilter.SelectSite.length === 0) {
      alert("Please Choose at least one Site");
      return false;
    }
    let Data = {
      SelectSite: stateFilter.SelectSite,
      Currency: parseInt(stateFilter.Currency),
      CategoryId: parseInt(stateFilter.Category),
      StartDate: moment.utc(stateFilter.StartDate).unix(),
      EndDate: moment.utc(stateFilter.EndDate).unix(),
      PageNumber: PageNumber,
      PageSize: 10
    };
    console.log('Data',Data);
    api
      .post("/api/BonusManage/GetLeaderboardExtraPointList", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          setDatas({
            PageInfo: {
              totalPages: res.data.totalPages,
              pageNumber: 1,
              rowCount: res.data.rowCount,
              currencyAmount: res.data.currencyAmount
            },
            Datas: [...res.data.data],
            FilterInfo: {
              ...stateFilter,
            },
          });
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };
  const Add_ExtraPointItem = () => {
    let Data = {
      ActiveDate :moment(stateExtraPointitem.ActiveDate).unix(),
      CategoryId:parseInt(stateExtraPointitem.CategoryId),
      UserName:stateExtraPointitem.UserName,
      ExtraPoint:parseFloat(stateExtraPointitem.ExtraPoint)
    };
    console.log('Add_ExtraPointItem Data',Data,"stateExtraPointitem",stateExtraPointitem);
    api
      .post("/api/BonusManage/SetLeaderboardExtraPoint", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          Filter_Submit();
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };
  

  
  return (
    <div className="container-fluid">
      <h4 className="page-name">LeaderBoard ExtraPoint</h4>
      {stateDatas.IsAdd ? (
        <EditPanel
          siteList={siteList}
          ClickBack={() => {
            setDatas({
              ...stateDatas,
              IsAdd: false,
            });
          }}
          Datas={stateDatas.SettingDatas}
          IsAdd={stateDatas.IsAdd}
          CategoryList = {CategoryList}
          onChange={addExtraPoint_OnChange}
          Add_ExtraPointItem = {Add_ExtraPointItem}
        />
      ) :
      (
        <>
          <Filter
            filterInfo={stateFilter}
            onChange={Filter_OnChange}
            onChangeCheckBox={Filter_CheckBox_OnChange}
            onChangeAllCheckBox={Filter_CheckBox_CheckAll}
            ClickSubmit={Filter_Submit}
            ClickAdd={ClickAdd}
            SiteList={siteList}
            CategoryList={CategoryList}
            datePickeronChange={DatePicker_OnChange}
          />
          {stateDatas.Datas === null ? null : (
            <Result
              {...stateDatas}
              ChangePage={Result_ChangePage}
              ClickAdd={ClickAdd}
            />
          )}
      </>
      )}
    </div>
  );
};

export default App;

if (process.env.NODE_ENV === "production") {
  ReactDOM.render(<App />, document.getElementById("content"));
}
