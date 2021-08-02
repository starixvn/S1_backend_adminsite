import React, { useEffect, useRef, useReducer, useState } from "react";
import ReactDOM from "react-dom";
import WebAPI from "../../../module/WebAPI";
import moment from "moment";
import Edit from "./edit";
import qs from "qs";
import {
  defActionType,
  defPanelType,
  reducer,
  initState,
} from "./index.reducer";
import Main from "./index.main";
import { api } from "../../../module/WebAPI";

const App = () => {
  const [state, dispatch] = useReducer(reducer, initState);
  const [processing, setProcessing] = useState(false);
  const [siteList, setSiteList] = useState();
  const isMountedRef = useRef(null);
  useEffect(() => {
    isMountedRef.current = true;
    return () => (isMountedRef.current = false);
  });
  const [stateFilter, setFilter] = useState({
    SelectSite: [],
    StartDate: moment().add(-10, "days").format("l"),
    EndDate: moment().format("l"),
    Status: "0",
  });
  const [stateDatas, setDatas] = useState({
    PageInfo: null,
    FilterInfo: {
      SelectSite: []
    },
  });

  useEffect(() => {
    let param = {
      Url: "Common/AdminUserSiteList",
    };
    WebAPI.PostData(param).then((res) => {
      if (res.data.errorCode === 0) {
        setSiteList(res.data.data);
      } else {
        alert(res.data.message ?? "failed to get data.");
      }
    });

    WebAPI.PostData({
      Url: "News/GetSiteListByLoginUser",
    }).then((res) => {
      if (isMountedRef.current) {
        if (res.data.errorCode === 0) {
          dispatch({
            type: defActionType.SET_SITE_LIST,
            siteList: res.data.data,
          });
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      }
    });
  }, []);

  const Filter_Submit = () => {
    if (stateFilter.SelectSite.length === 0) {
      alert("Please Choose at least one Site");
      return false;
    }
    let Data = {
      SelectSite: stateFilter.SelectSite,
      StartDate: moment(stateFilter.StartDate).unix(),
      EndDate: moment(stateFilter.EndDate).add(1, "days").unix() - 1,
      Status: parseInt(stateFilter.Status, 10),
      PageNumber: 1,
      PageSize: 10,
    };
    api
      .post("/api/News/GetNewsList", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          dispatch({
            type: defActionType.SET_DATA_LIST,
            dataList: res.data.data,
          });
          setDatas({
            PageInfo: {
              totalPages: res.data.totalPages,
              pageNumber: 1,
              rowCount: res.data.rowCount,
            },
            FilterInfo: {
              SelectSite: stateFilter.SelectSite,
              StartDate: stateFilter.StartDate,
              EndDate: stateFilter.EndDate,
              Status: stateFilter.Status,
              PageNumber: Data.PageNumber,
              PageSize: 10,
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

  const ChangePage = (PageNumber) => {
    if (stateFilter.SelectSite.length === 0) {
      alert("Please Choose at least one Site");
      return false;
    }
    let Data = {
      SelectSite: stateDatas.FilterInfo.SelectSite,
      StartDate: moment(stateDatas.FilterInfo.StartDate).unix(),
      EndDate: moment(stateDatas.FilterInfo.EndDate).add(1, "days").unix() - 1,
      Status: parseInt(stateDatas.FilterInfo.Status, 10),
      PageNumber: PageNumber,
      PageSize: 10,
    };
    api
      .post("/api/News/GetNewsList", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          dispatch({
            type: defActionType.SET_DATA_LIST,
            dataList: res.data.data,
          });
          setDatas({
            ...stateDatas,
            PageInfo: {
              totalPages: res.data.totalPages,
              pageNumber: PageNumber,
              rowCount: res.data.rowCount,
            },
            FilterInfo: {
              SelectSite: stateDatas.FilterInfo.SelectSite,
              StartDate: stateDatas.FilterInfo.StartDate,
              EndDate: stateDatas.FilterInfo.EndDate,
              Status: stateDatas.FilterInfo.Status,
              PageNumber: PageNumber,
              PageSize: 10,
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
  const ClickAdd = () => {
    dispatch({
      type: defActionType.GOTO_ADD,
    });
  };

  const UpdateStatus = (idx) => {
    let _data = state.dataList[idx];
    let msg = _data.status === 1 ? "Disable this News?" : "Enable this News?";
    if (_data && window.confirm(msg)) {
      let newStatus = _data.status === 1 ? 0 : 1;
      let param = {
        Url: "News/SetStatus",
        Data: qs.stringify({
          NewsId: _data.newsId,
          Status: newStatus,
        }),
      };
      WebAPI.PostData(param).then((res) => {
        if (isMountedRef.current) {
          if (res.data.errorCode === 0) {
            dispatch({
              type: defActionType.SET_STATUS,
              idx: idx,
              status: newStatus,
              lastUpdateUser: res.data.data.lastUpdateUser,
              lastUpdateTime: res.data.data.lastUpdateTime,
            });
          } else {
            alert(res.data.message ?? "failed to get data.");
          }
        }
      });
    }
  };

  const ClickBack = () => {
    dispatch({ type: defActionType.CLICK_BACK });
  };

  const ClickSubmit = (data) => {
    if (!processing) {
      if (isMountedRef.current) {
        setProcessing(true);
      }

      let title = {};
      for (let lang in data.title) {
        if (data.title[lang]) {
          title[lang] = data.title[lang];
        }
      }

      let content = {};
      for (let lang in data.content) {
        if (data.content[lang]) {
          content[lang] = data.content[lang];
        }
      }

      let imageURL = {};
      for (let lang in data.imageURL) {
        if (data.imageURL[lang]) {
          imageURL[lang] = data.imageURL[lang];
        }
      }

      let _datas = {
        SiteId: data.siteId,
        Title: JSON.stringify(title),
        Content: JSON.stringify(content),
        ImageURL: JSON.stringify(imageURL),
        Status: data.status,
        UnixPostTime: moment(data.UnixPostTime).unix(),
        Sort: data.sort,
        NewsId: data.newsId,
      };

      WebAPI.PostData({
        Url: "News/SetNews",
        Data: qs.stringify({
          IsAdd: state.panelType === defPanelType.Add,
          newsDatas: _datas,
        }),
      }).then(
        (res) => {
          if (isMountedRef.current) {
            if (res.data.errorCode === 0) {
              if (stateDatas.FilterInfo.SelectSite.length === 0) {
                dispatch({
                  type: defActionType.SET_DATA_LIST,
                  dataList: [],
                });
              } else {
                let Data = {
                  SelectSite: stateDatas.FilterInfo.SelectSite,
                  StartDate: moment(stateDatas.FilterInfo.StartDate).unix(),
                  EndDate: moment(stateDatas.FilterInfo.EndDate).add(1, "days").unix() - 1,
                  Status: parseInt(stateDatas.FilterInfo.Status, 10),
                  PageNumber: stateDatas.FilterInfo.PageNumber,
                  PageSize: 10,
                };
                api
                  .post("/api/News/GetNewsList", Data)
                  .then(function (res) {
                    if (res.data.errorCode === 0) {
                      dispatch({
                        type: defActionType.SET_DATA_LIST,
                        dataList: res.data.data,
                      });
                      setDatas({
                        ...stateDatas,
                        PageInfo: {
                          totalPages: res.data.totalPages,
                          pageNumber: stateDatas.FilterInfo.PageNumber,
                          rowCount: res.data.rowCount,
                        }
                      });
                    } else {
                      alert(res.data.message ?? "failed to get data.");
                    }
                  })
                  .catch(function (error) {
                    alert(error);
                  });
              }
              setProcessing(false);
            } else {
              alert(res.data.message ?? "failed to set data.");
              setProcessing(false);
            }
          }
        },
        (err) => {
          if (isMountedRef.current) {
            alert("API Server Error.");
            setProcessing(false);
          }
        }
      );
    }
  };

  const ClickEdit = (idx) => {
    if (!processing) {
      if (isMountedRef.current) {
        setProcessing(true);
      }

      WebAPI.PostData({
        Url: "News/GetNewsById",
        Data: qs.stringify({
          NewsId: state.dataList[idx].newsId,
        }),
      }).then(
        (res) => {
          if (isMountedRef.current) {
            if (res.data.errorCode === 0) {
              dispatch({
                type: defActionType.GOTO_EDIT,
                data: res.data.data,
              });
            }
            setProcessing(false);
          }
        },
        (err) => {
          if (isMountedRef.current) {
            alert("API Server Error.");
            setProcessing(false);
          }
        }
      );
    }
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

  const Filter_CheckBox_CheckAll = (v, dataColumn, options) => {
    if (v !== "All") return false;
    let currentTab =
      stateFilter[dataColumn].length === options.length
        ? []
        : options.map((o) => {
            return o.value;
          });
    setFilter({
      ...stateFilter,
      [dataColumn]: currentTab,
    });
  };

  const Filter_OnChange = (v, dataColumn) => {
    setFilter({
      ...stateFilter,
      [dataColumn]: v,
    });
  };

  return state.panelType === defPanelType.Main ? (
    <Main
      ClickAdd={ClickAdd}
      ClickEdit={ClickEdit}
      UpdateStatus={UpdateStatus}
      dataList={state.dataList}
      siteList={state.siteList}
      newsiteList={siteList}
      stateFilter={stateFilter}
      Filter_CheckBox_OnChange={Filter_CheckBox_OnChange}
      Filter_CheckBox_CheckAll={Filter_CheckBox_CheckAll}
      Filter_OnChange={Filter_OnChange}
      Filter_Submit={Filter_Submit}
      stateDatas={stateDatas}
      ChangePage={ChangePage}
    />
  ) : (
    <Edit
      datas={state.data}
      siteList={state.siteList}
      ClickBack={ClickBack}
      ClickSubmit={ClickSubmit}
      isAdd={state.panelType === defPanelType.Add}
      processing={processing}
    />
  );
};

export default App;

if (process.env.NODE_ENV === "production") {
  ReactDOM.render(<App />, document.getElementById("content"));
}
