import { number_format } from "../../commJS/common";
import moment from 'moment';

const BonusInfoStatus = {
    1: <span className="status status-active">Running</span>,
    2: <span className="status status-suspend">Canceled</span>,
    3: <span className="status status-closed">Expired</span>,
    4: <span className="status status-initial">Reached</span>,
}

const Result = (props) => {
  let rows =
    props.Datas.length === 0
      ? null
      : props.Datas.map((d, dIdx) => {
          return (
            <tr key={`result-${dIdx}`}>
              <td>{d.bonusCode}</td>
              <td>{d.description}</td>
              <td>{number_format(d.bonusAmount)}</td>
              <td>{number_format(d.lockedAmount)}</td>
              <td>{number_format(d.custRollOver.rollOver)}</td>
              <td>{number_format(d.custRollOver.turnOver)}</td>
              <td>{moment(d.claimDate).format('l LTS')}</td>
              <td>{BonusInfoStatus[d.bonusInfoStatus]}</td>
              <td>{d.bonusInfoStatus == 2 ? <button className="btn btn-primary" onClick={props.CancelBonus.bind(this, d.bonusId)}>Cancel Bonus</button> : null}</td>
            </tr>
          );
        });
  return (
    <>
      <div className="table-responsive-md table">
        <span class="label">Promotion</span>
        <table className="table table-striped">
          <thead>
            <tr>
              <th scope="col">BonusCode</th>
              <th scope="col">Description</th>
              <th scope="col">Bonus Amount</th>
              <th scope="col">Locked Amount</th>
              <th scope="col">Rollover</th>
              <th scope="col">Turnover</th>
              <th scope="col">ClaimDate</th>
              <th scope="col">Status</th>
              <th scope="col"></th>
            </tr>
          </thead>
          {rows}
        </table>
      </div>
    </>
  );
};

export default Result;
