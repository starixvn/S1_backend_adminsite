import Filter from "./Filter";
import Result from "./Result";
import { getUrlParameter } from "../../commJS/common";
import moment from "moment-timezone";
const Panel = () => {
  moment.tz.setDefault("Asia/Taipei");
  const [stateDatas, setDatas] = React.useState();
  const [isloading,setisloading] = React.useState(false);
  let CustId = getUrlParameter("custId");
  const onSubmit = (filterDatas) => {
    console.log(filterDatas.GameTypes);
    setisloading(true);
    axios
      .post("/api/CustManage/GetCustTurnOverReport", {
        CustId: parseInt(CustId),
        StartTime: moment(filterDatas.StartTime).unix(),
        EndTime: moment(filterDatas.EndTime).unix(),
        GameTypes: filterDatas.GameTypes,
      })
      .then((res) => {
        let result = res.data;
        if (result.errorCode == 0) {
          setDatas(result.data);
          setisloading(false);
        } else {
          alert(result.message);
          setisloading(false);
        }
      })
      .catch((error) => {
        alert(error);
        setisloading(false);
      });
  };

  return (
    <>
      <Filter ClickSubmit={onSubmit} Isloading={isloading}/>
      {stateDatas === undefined ? null : <Result Datas={stateDatas} />}
    </>
  );
};

export default Panel;
